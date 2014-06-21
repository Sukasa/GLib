Imports GLib.Win32
Imports System.IO

Namespace GLib
    Public Class GSeriesController
        Private Shared G15Interface As New GLib.HardwareInterface

        Private KeyboardHandle As Integer
        Private DeviceChangeNotifier As frmDeviceChangeCatcher
        Private CachedKeyboardList As New List(Of GSeriesKeyboard)

        Public Delegate Sub DeviceChangeEventHandler()

        Public Event DeviceChange As DeviceChangeEventHandler

        Private Shared SingletonInstance As GSeriesController

        Private Sub New()
            DeviceChangeNotifier = New frmDeviceChangeCatcher(Me)
            DeviceChangeNotifier.Show() 'Show then hide, so that the form starts capturing events (Is there a less hacky way of doing this?)
            DeviceChangeNotifier.Hide()

            Dim RawInputDevice As New RAWINPUTDEVICE
            RawInputDevice.UsagePage = HIDUsagePage.Generic
            RawInputDevice.Usage = HIDUsage.Keyboard
            RawInputDevice.WindowHandle = DeviceChangeNotifier.Handle
            RawInputDevice.Flags = RawInputDeviceFlags.InputSink Or RawInputDeviceFlags.NoLegacy Or RawInputDeviceFlags.AppKeys

            RegisterRawInputDevices(New RAWINPUTDEVICE() {RawInputDevice}, 1, Marshal.SizeOf(RawInputDevice))
        End Sub

        Public Sub InputTick()
            System.Windows.Forms.Application.DoEvents() ' Pump message queue.  Necessary when not deployed in a winforms project.
        End Sub

        Friend Function ProcessRawInput(ByVal Message As Windows.Forms.Message) As Boolean
            Dim RequiredSize As Integer = 0
            GetRawInputData(Message.LParam, RID_INPUT, IntPtr.Zero, RequiredSize, Marshal.SizeOf(GetType(RawInputHeader)))
            Dim RawInputPtr As IntPtr = Marshal.AllocHGlobal(RequiredSize)
            GetRawInputData(Message.LParam, RID_INPUT, RawInputPtr, RequiredSize, Marshal.SizeOf(GetType(RawInputHeader)))
            Dim Raw As RawInput = Marshal.PtrToStructure(RawInputPtr, GetType(RawInput))

            If Raw.Header.Device <> 0 Then 'Media keys don't list a device in Raw Input.  A workaround is in place via the KeyboardMediaEndpoint class.
                GetRawInputDeviceInfo(Raw.Header.Device, RIDI_DEVICENAME, IntPtr.Zero, RequiredSize)
                Dim DeviceStringPtr As IntPtr = Marshal.AllocHGlobal(RequiredSize * Marshal.SystemDefaultCharSize)
                Dim CharacterCount As Integer = GetRawInputDeviceInfo(Raw.Header.Device, RIDI_DEVICENAME, DeviceStringPtr, RequiredSize)

                If CharacterCount > 0 Then 'It's possible for there to be no originating device string, e.g. when using remote desktop
                    Dim DeviceString As String = Marshal.PtrToStringAnsi(DeviceStringPtr, CharacterCount)
                    Dim K As GSeriesKeyboard = MultiPartResolver.Instance.GetAssociatedKeyboard(DeviceString)
                    If K IsNot Nothing Then
                        K.KeyEvent(Raw.RawData.Keyboard.Message, Raw.RawData.Keyboard.VirtualKey)
                    End If
                End If

                Marshal.FreeHGlobal(DeviceStringPtr)
            End If
            Marshal.FreeHGlobal(RawInputPtr)
            Return True
        End Function

        Public Shared Function Instance() As GSeriesController
            If SingletonInstance Is Nothing Then
                SingletonInstance = New GSeriesController
            End If
            Return SingletonInstance
        End Function

        Public ReadOnly Property Keyboards() As List(Of GSeriesKeyboard)
            Get
                If CachedKeyboardList.Count = 0 Then
                    If G15Interface.CurrentKeyboards.Count = 0 Then
                        G15Interface.LocateKeyboards()
                    End If
                    Dim List As New List(Of GSeriesKeyboard)(G15Interface.CurrentKeyboards)
                    List.RemoveAll(Function(T) T.GetType Is GetType(KeyboardMediaEndpoint) OrElse Not T.Connected) 'Apply Filter
                    CachedKeyboardList = List 'And cache results
                End If
                Dim TempList As New List(Of GSeriesKeyboard)(CachedKeyboardList.Count)
                TempList.AddRange(CachedKeyboardList)
                Return TempList ' Don't return the cached list, so that client code can't accidentally modify it
            End Get
        End Property

        Friend Sub SignalDeviceChange()
            CachedKeyboardList.Clear() 'Clear filtered-keyboards cache
            G15Interface.LocateKeyboards()
            RaiseEvent DeviceChange()
        End Sub
    End Class
End Namespace
