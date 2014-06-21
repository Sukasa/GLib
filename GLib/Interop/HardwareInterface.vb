Imports GLib.Win32
Imports System.Security.AccessControl
Imports System.Threading
Imports Microsoft.Win32.SafeHandles
Imports System.Windows.Forms
Imports System.Linq

Namespace GLib
    Public Class HardwareInterface
        Friend CurrentKeyboards As New List(Of GSeriesKeyboard)

        Private PreviousKeyboards As New List(Of GSeriesKeyboard)
        Private Keyboards As Dictionary(Of String, Type) = New Dictionary(Of String, Type)

        Friend Sub New()
            GSeriesKeyboard.HardwareInterface = Me
            InitDictionary()
        End Sub

        Public Function GetKeyInfo(ByVal KeyboardPath As String) As Object

            Dim KeyboardHandle As Integer = OpenInterface(KeyboardPath)
            Dim BytesRead As Integer
            Dim Buffer(6) As Byte
            ReadFile(KeyboardHandle, Buffer, Buffer.Length, BytesRead, Nothing)

            CloseInterface(KeyboardHandle)
            Return Buffer
        End Function

        Friend Shared Function OpenInterface(ByVal DevicePath As String) As Integer

            Dim SecurityData As New SECURITY_ATTRIBUTES()
            Dim Security As New DirectorySecurity()
            Dim DescriptorBinary As Byte() = Security.GetSecurityDescriptorBinaryForm()
            Dim SecurityDescriptorPtr As IntPtr = Marshal.AllocHGlobal(DescriptorBinary.Length)

            SecurityData.nLength = Marshal.SizeOf(SecurityData)
            Marshal.Copy(DescriptorBinary, 0, SecurityDescriptorPtr, DescriptorBinary.Length)
            SecurityData.lpSecurityDescriptor = SecurityDescriptorPtr

            Dim Handle As Integer = Win32.CreateFile(DevicePath, GENERIC_READ Or GENERIC_WRITE, _
                                                     FILE_SHARE_READ Or FILE_SHARE_WRITE, SecurityData, _
                                                     OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0)

            Marshal.FreeHGlobal(SecurityDescriptorPtr)

            Return Handle
        End Function

        Friend Shared Sub CloseInterface(ByRef fileHandle As Integer)
            Win32.CloseHandle(fileHandle)
        End Sub

        Friend Sub LocateKeyboards()
            PreviousKeyboards.AddRange(From Keyboard As GSeriesKeyboard In CurrentKeyboards Where Not PreviousKeyboards.Contains(Keyboard) Select Keyboard)
            MultiPartResolver.Instance.ClearPartsList()

            For Each Keyboard As GSeriesKeyboard In CurrentKeyboards
                Keyboard.Close()
            Next

            CurrentKeyboards = New List(Of GSeriesKeyboard)

            Dim HIDGuid As Guid
            Win32.HidD_GetHidGuid(HIDGuid)

            Dim LCDGuid As Guid = New Guid("{b91b7968-6435-4966-8928-79bf082e3e30}")

            Dim ConnectedDevices As IntPtr = Win32.SetupDiGetClassDevs(Nothing, Nothing, 0, DIGCF_DEVICEINTERFACE Or DIGCF_PRESENT Or DIGCF_ALLCLASSES)

            Dim DeviceIndex As Integer = 0
            Dim InterfaceInfo As SP_DEVICE_INTERFACE_DATA
            InterfaceInfo.cbSize = Marshal.SizeOf(InterfaceInfo)

            While Win32.SetupDiEnumDeviceInterfaces(ConnectedDevices, 0, HIDGuid, DeviceIndex, InterfaceInfo)

                Dim RequiredDataSize As Integer

                Dim DeviceInfo As New SP_DEVINFO_DATA
                DeviceInfo.cbSize = Marshal.SizeOf(DeviceInfo)

                Win32.SetupDiGetDeviceInterfaceDetail(ConnectedDevices, InterfaceInfo, IntPtr.Zero, 0, RequiredDataSize, DeviceInfo)

                Dim MemPtr As IntPtr = Marshal.AllocHGlobal(RequiredDataSize)

                'Okay, this is pretty hacky but I really don't know of a better way to write this code.
                'If the binary is compiled as x64, I have to declare the size of the struct as
                '8 bytes instead of 6 due to packing (8 byte vs. 1 byte)
                Marshal.WriteInt32(MemPtr, If(IntPtr.Size = 8, 8, 4 + Marshal.SystemDefaultCharSize)) ' 4 = System.Int32.Size

                Win32.SetupDiGetDeviceInterfaceDetail(ConnectedDevices, InterfaceInfo, MemPtr, RequiredDataSize, RequiredDataSize, DeviceInfo)

                Dim DeviceCode As New IntPtr(MemPtr.ToInt32 + 4) ' 4 = System.Int32.Size
                Dim DeviceString As String = Marshal.PtrToStringAuto(DeviceCode)

                MultiPartResolver.Instance.AddPath(DeviceString, DeviceInfo.DevInst)

                Dim Keyboard As GSeriesKeyboard = Nothing
                For I As Integer = 0 To Keyboards.Keys.Count - 1
                    If (DeviceString.IndexOf(Keyboards.Keys(I)) > 0) Then
                        Keyboard = PreviousKeyboards.Find(Function(T) T.DevicePath = DeviceString)
                        If Keyboard Is Nothing Then
                            Keyboard = Activator.CreateInstance(Keyboards.Values(I), True)
                            Keyboard.Init(DeviceString, True, DeviceInfo.DevInst)
                        Else
                            Keyboard.Init(DeviceString, False, DeviceInfo.DevInst)
                        End If
                        CurrentKeyboards.Add(Keyboard)
                        If Keyboard.GetType() IsNot GetType(KeyboardMediaEndpoint) Then ' Registering a KME can cause it to find ITSELF as master, leading to an infinite recursive loop and stack overflow...
                            MultiPartResolver.Instance.RegisterConnectedKeyboard(Keyboard) ' ...So don't.
                        End If
                        Exit For
                    End If
                Next I

                Marshal.FreeHGlobal(MemPtr)
                DeviceIndex += 1
            End While
            Dim ee As Integer = Marshal.GetLastWin32Error()
            Win32.SetupDiDestroyDeviceInfoList(ConnectedDevices)

            For Each Keyboard As GSeriesKeyboard In CurrentKeyboards
                Keyboard.DeviceChangeComplete()
            Next

        End Sub

        Public Shared Function GetParentDeviceInstanceHandle(ByVal DeviceInstance As Integer) As UInteger
            Dim Previous As UInteger = Nothing

            CM_Get_Parent(Previous, DeviceInstance, 0)

            Return Previous
        End Function

        Public Shared Function GetSiblingDeviceInstanceHandle(ByVal DeviceInstance As Integer) As UInteger
            Dim Previous As UInteger = Nothing

            CM_Get_Sibling(Previous, DeviceInstance, 0)

            Return Previous
        End Function

        Public Shared Function GetChildDeviceInstanceHandle(ByVal DeviceInstance As Integer) As UInteger
            Dim Previous As UInteger = Nothing

            CM_Get_Child(Previous, DeviceInstance, 0)

            Return Previous
        End Function

        Public Shared Function GetDevicePathFromInstance(ByVal DeviceInstance As UInteger) As String
            Dim Bytes As Integer = 0
            CM_Get_Device_ID_Size(Bytes, DeviceInstance, 0)
            Dim StrPtr As IntPtr = Marshal.AllocHGlobal((Bytes + 1) * Marshal.SystemDefaultCharSize)

            CM_Get_Device_ID(DeviceInstance, StrPtr, Bytes, 0)

            Dim DevicePath As String = Marshal.PtrToStringAuto(StrPtr, Bytes)

            Marshal.FreeHGlobal(StrPtr)

            Return DevicePath
        End Function

        Friend Sub ConfirmExternalIOHandle(ByVal Keyboard As GSeriesKeyboard)
            If Keyboard.ExternalIOHandle < 1 Then Keyboard.ExternalIOHandle = OpenInterface(Keyboard.DevicePath)
        End Sub

        Friend Function GetFeature(ByVal Keyboard As GSeriesKeyboard, ByRef FeatureData As Byte()) As Byte()
            If FeatureData Is Nothing OrElse Not Keyboard.Connected Then Return Nothing
            ConfirmExternalIOHandle(Keyboard)
            Win32.HidD_GetFeature(Keyboard.ExternalIOHandle, FeatureData, FeatureData.Length)
            Return FeatureData
        End Function

        Friend Sub SetFeature(ByVal Keyboard As GSeriesKeyboard, ByVal FeatureData() As Byte)
            If FeatureData Is Nothing OrElse Not Keyboard.Connected Then Return
            ConfirmExternalIOHandle(Keyboard)
            Dim Success As Boolean = Win32.HidD_SetFeature(Keyboard.ExternalIOHandle, FeatureData, FeatureData.Length)
            Dim ee As Integer = Marshal.GetLastWin32Error
        End Sub

        Public Sub WriteData(ByVal Keyboard As GSeriesKeyboard, ByVal Data() As Byte)
            If Data Is Nothing OrElse Not Keyboard.Connected Then Return
            ConfirmExternalIOHandle(Keyboard)
            Dim NumberBytesWritten As Integer = 0

            Dim Success As Boolean = Win32.WriteFile(Keyboard.ExternalIOHandle, Data, Data.Length, NumberBytesWritten, Nothing)
            Dim ee As Integer = Marshal.GetLastWin32Error


        End Sub

        Private Sub InitDictionary()

            'Don't forget to add the necessary paths to MultiPartResolver.vb (@ top of file)

            Keyboards.Add("vid_046d&pid_c222&col02", GetType(G15v1Keyboard)) 'G15v1 (Untested, I don't have one)
            Keyboards.Add("vid_046d&pid_c227&col02", GetType(G15v2Keyboard)) 'G15v2
            Keyboards.Add("vid_046d&pid_c226&mi_01&col01", GetType(KeyboardMediaEndpoint))

            Keyboards.Add("vid_046d&pid_c22d&mi_01&col02", GetType(G510Keyboard)) 'G510, Audio Disabled
            Keyboards.Add("vid_046d&pid_c22d&mi_01&col01", GetType(KeyboardMediaEndpoint))

            Keyboards.Add("vid_046d&pid_c22e&mi_01&col02", GetType(G510Keyboard)) 'G510, Audio Enabled
            Keyboards.Add("vid_046d&pid_c22e&mi_01&col01", GetType(KeyboardMediaEndpoint))

            Keyboards.Add("vid_046d&pid_c229", GetType(G19Keyboard)) 'G19 (Partial)
        End Sub

    End Class
End Namespace

