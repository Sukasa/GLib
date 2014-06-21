Imports GLib.Win32
Imports Microsoft.Win32.SafeHandles
Imports System.Drawing.Imaging
Imports GLib.Enumerations
Imports System.Threading
Imports System.Windows.Forms
Imports System.Resources
Imports System.Globalization

Namespace GLib
    Public MustInherit Class GSeriesKeyboard

        Private HandleOpen As Boolean = True

        Public Shared HardwareInterface As GLib.HardwareInterface

        Friend DevicePath As String
        Friend DeviceInstance As UInteger
        Public ExternalIOHandle As Integer
        Public InternalIOHandle As Integer
        Friend ImageWidth As Integer = 160
        Friend ImageHeight As Integer = 43

        Private PressedGKeys As List(Of GKeys) = New List(Of GKeys)()
        Private PressedSKeys As List(Of SKeys) = New List(Of SKeys)()
        Private PressedMKeys As List(Of MKeys) = New List(Of MKeys)()
        Private PressedAKeys As List(Of Keys) = New List(Of Keys)()
        Private NewPressedGKeys As List(Of GKeys) = New List(Of GKeys)()
        Private NewPressedSKeys As List(Of SKeys) = New List(Of SKeys)()
        Private NewPressedMKeys As List(Of MKeys) = New List(Of MKeys)()
        Private NewPressedRKeys As List(Of Keys) = New List(Of Keys)()

        Public Event GKeyDown(ByVal Sender As GSeriesKeyboard, ByVal Key As GKeys)
        Public Event GKeyUp(ByVal Sender As GSeriesKeyboard, ByVal Key As GKeys)
        Public Event SKeyDown(ByVal Sender As GSeriesKeyboard, ByVal Key As SKeys)
        Public Event SKeyUp(ByVal Sender As GSeriesKeyboard, ByVal Key As SKeys)
        Public Event MKeyDown(ByVal Sender As GSeriesKeyboard, ByVal Key As MKeys)
        Public Event MKeyUp(ByVal Sender As GSeriesKeyboard, ByVal Key As MKeys)
        Public Event RKeyDown(ByVal Sender As GSeriesKeyboard, ByVal Key As Keys)
        Public Event RKeyUp(ByVal Sender As GSeriesKeyboard, ByVal Key As Keys)
        Public Event RKeyPressed(ByVal Sender As GSeriesKeyboard, ByVal Key As Keys)

        Friend MustOverride Function GetSetFeatureData(ByVal FeatureType As FeatureType, ByVal FeatureValue As Object) As Byte()
        Friend MustOverride Function GetGetFeatureData(ByVal FeatureType As FeatureType) As Byte()
        Friend MustOverride Function InterpretGetFeatureData(ByVal FeatureType As FeatureType, ByVal FeatureData As Byte()) As Object

        Friend MustOverride Function DecodeSKeys(ByVal Code() As Byte) As List(Of SKeys)
        Friend MustOverride Function DecodeGKeys(ByVal Code() As Byte) As List(Of GKeys)
        Friend MustOverride Function DecodeMKeys(ByVal Code() As Byte) As List(Of MKeys)
        Friend MustOverride Function DecodeAKeys(ByVal Code() As Byte) As List(Of Keys)
        Friend MustOverride Sub DispatchKeyboardUpdate(ByVal Code() As Byte)

        Friend MustOverride Sub SpecificInit()

        Private Delegate Sub ReadReportDelegate()
        Private Delegate Sub KeyReadCallbackDelegate(ByVal Report As Object)
        Private Delegate Function ReadDataDelegate() As Byte()

        Public Overridable ReadOnly Property LCDType() As KeyboardLCDTtype
            Get
                Return KeyboardLCDTtype.NoLCD
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetType.Name
        End Function

        Public ReadOnly Property KeyboardPath() As String
            Get
                Return DevicePath
            End Get
        End Property

        Public ReadOnly Property Connected() As Boolean
            Get
                Return HandleOpen
            End Get
        End Property

        Private Shared Function GetKeyArray(ByVal DataString As String) As Keys()
            Dim Lines As String() = DataString.Split(Environment.NewLine)
            Dim KeysArray(255) As Keys
            For Each Line As String In Lines
                Dim Parts() As String = Line.Split(" ")
                KeysArray(Integer.Parse(Parts(0))) = [Enum].Parse(GetType(Keys), Parts(1))
            Next
            Return KeysArray
        End Function

        Protected Function IsValidLCDImage(ByVal Image As Bitmap) As Boolean
            If Image Is Nothing OrElse Image.PixelFormat <> PixelFormat.Format32bppArgb Then Return False
            Select Case LCDType
                Case KeyboardLCDTtype.NoLCD
                    Return False
                Case KeyboardLCDTtype.Colour320x240
                    Return Image.Width = 320 AndAlso Image.Height = 240
                Case KeyboardLCDTtype.Monochrome160x43
                    Return Image.Width = 160 AndAlso Image.Height >= 43 AndAlso Image.Height <= 48
            End Select
        End Function

        Protected Function FormatLCDColour320(ByVal SourceBitmap As Bitmap) As Byte()
            Dim LCDData As Byte() = New Byte((320 * 240 * 2) + 511) {}

            LCDData(0) = 16

            Return LCDData

            'Dim LCDData As Byte() = New Byte(&H25A00 - 1) {}
            'Array.Copy(G19LCDDataHeader, LCDData, 512) 'G19LCDHeader.vb because it's a mess
            'Dim DataPointer As Integer = 512
            'For Y As Integer = 0 To 239
            '    For X As Integer = 0 To 319
            '        Dim PixelColour As Color = SourceBitmap.GetPixel(X, Y) 'Probably not the most efficient way to do this...
            '        Dim Colour As UInt16 = (CUShort(PixelColour.R >> 3) << 11) Or (CUShort(PixelColour.G >> 2) << 5) Or CUShort(PixelColour.B >> 3)
            '        LCDData(DataPointer) = Colour >> 8
            '        LCDData(DataPointer + 1) = Colour And 255
            '        DataPointer += 2
            '    Next
            'Next
            'Return LCDData
        End Function

        Protected Function FormatLCDBW160(ByVal SourceBitmap As Bitmap) As Byte()
            Dim LCDData As Byte() = New Byte(991) {}
            Dim Image(640 * 48) As Byte 'Temp array for image conversion.  Adds additional 5 lines of unused pixels to avoid an out-of-bounds error
            Dim BitmapData As BitmapData = SourceBitmap.LockBits(New Rectangle(0, 0, 160, 43), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
            Marshal.Copy(BitmapData.Scan0, Image, 0, (640 * 43))

            LCDData(0) = &H3 'Set-LCD
            Dim output As Integer = 32 'First byte of image data starts at byte 32; 960 bytes of image data
            Dim ImageOffset As Integer = 0
            For Row As Integer = 0 To 5
                For Column As Integer = 0 To (SourceBitmap.Width << 2) - 1 Step 4

                    Dim r As Integer = _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 0) And &H80) >> 7) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 1) And &H80) >> 6) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 2) And &H80) >> 5) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 3) And &H80) >> 4) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 4) And &H80) >> 3) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 5) And &H80) >> 2) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 6) And &H80) >> 1) Or _
                    ((Image(ImageOffset + Column + BitmapData.Stride * 7) And &H80) >> 0)

                    LCDData(output) = CByte(r)

                    output += 1
                Next
                ImageOffset += BitmapData.Stride * 8
            Next
            SourceBitmap.UnlockBits(BitmapData)
            Return LCDData
        End Function

        Friend Sub Init(ByVal DeviceID As String, ByVal IsNew As Boolean, ByVal DevInst As UInteger)
            DevicePath = DeviceID
            DeviceInstance = DevInst

            ExternalIOHandle = HardwareInterface.OpenInterface(DeviceID)
            InternalIOHandle = HardwareInterface.OpenInterface(DeviceID)

            HandleOpen = ExternalIOHandle > 0 AndAlso InternalIOHandle > 0

            If Not HandleOpen Then Return

            SpecificInit()

            If IsNew Then ReadReport()
        End Sub

        Friend Sub Close()
            HandleOpen = False
            HardwareInterface.CloseInterface(ExternalIOHandle)
        End Sub

        Private Sub KeyReadCallback(ByVal Code As Byte())
            DispatchKeyboardUpdate(Code)
            ReadReport()
        End Sub

        Protected Sub ProcessMediaKeysStatusChange(ByVal Code() As Byte)
            Dim NewPressedAKeys As List(Of Keys) = DecodeAKeys(Code)

            For Each Key As Keys In PressedAKeys
                If Not NewPressedAKeys.Contains(Key) Then
                    KeyEvent(WM_KEYUP, Key)
                End If
            Next

            For Each Key As Keys In NewPressedAKeys
                If Not PressedAKeys.Contains(Key) Then
                    KeyEvent(WM_KEYDOWN, Key)
                End If
            Next

            PressedAKeys = NewPressedAKeys
        End Sub

        Protected Sub ProcessSpecialKeysStatusChange(ByVal Code() As Byte)

            NewPressedGKeys = DecodeGKeys(Code)
            NewPressedSKeys = DecodeSKeys(Code)
            NewPressedMKeys = DecodeMKeys(Code)

            For Each GKey As GKeys In PressedGKeys
                If Not NewPressedGKeys.Contains(GKey) Then
                    RaiseEvent GKeyUp(Me, GKey)
                End If
            Next

            For Each MKey As MKeys In PressedMKeys
                If Not NewPressedMKeys.Contains(MKey) Then
                    RaiseEvent MKeyUp(Me, MKey)
                End If
            Next

            For Each SKey As SKeys In PressedSKeys
                If Not NewPressedSKeys.Contains(SKey) Then
                    RaiseEvent SKeyUp(Me, SKey)
                End If
            Next



            For Each GKey As GKeys In NewPressedGKeys
                If Not PressedGKeys.Contains(GKey) Then
                    RaiseEvent GKeyDown(Me, GKey)
                End If
            Next

            For Each MKey As MKeys In NewPressedMKeys
                If Not PressedMKeys.Contains(MKey) Then
                    RaiseEvent MKeyDown(Me, MKey)
                End If
            Next

            For Each SKey As SKeys In NewPressedSKeys
                If Not PressedSKeys.Contains(SKey) Then
                    RaiseEvent SKeyDown(Me, SKey)
                End If
            Next



            PressedGKeys = NewPressedGKeys
            PressedMKeys = NewPressedMKeys
            PressedSKeys = NewPressedSKeys
        End Sub

        Private Sub ReadReport()
            While Not HandleOpen
                Thread.Sleep(100)
            End While
            ReadReport(AddressOf KeyReadCallback)
        End Sub

        Private Sub ReadReport(ByVal Callback As KeyReadCallbackDelegate)
            Dim ReadData As ReadDataDelegate = AddressOf ReadKeyData
            ReadData.BeginInvoke(AddressOf EndReadReport, New CallingInfo(ReadData, Callback))
        End Sub

        Private Function ReadKeyData() As Byte()
            Dim Data(31) As Byte
            Dim BytesRead As Integer
            ReadFile(InternalIOHandle, Data, 32, BytesRead, Nothing)

            Dim ee As Integer = GetLastError()
            Return Data
        End Function

        Private Class CallingInfo
            Public Sub New(ByVal CalledFunction As ReadDataDelegate, ByVal Callback As KeyReadCallbackDelegate)
                CalledDelegate = CalledFunction
                CallbackDelegate = Callback
            End Sub
            Public CalledDelegate As ReadDataDelegate
            Public CallbackDelegate As KeyReadCallbackDelegate
        End Class

        Private Shared Sub EndReadReport(ByVal AsyncResult As IAsyncResult)
            Dim AsyncState As CallingInfo = AsyncResult.AsyncState
            AsyncState.CallbackDelegate.Invoke(AsyncState.CalledDelegate.EndInvoke(AsyncResult))
        End Sub

        Public ReadOnly Property GKeysPressed() As List(Of GKeys)
            Get
                Return NewPressedGKeys
            End Get
        End Property

        Public ReadOnly Property SKeysPressed() As List(Of SKeys)
            Get
                Return NewPressedSKeys
            End Get
        End Property

        Public ReadOnly Property MKeysPressed() As List(Of MKeys)
            Get
                Return NewPressedMKeys
            End Get
        End Property

        Public ReadOnly Property KeysPressed() As List(Of Keys)
            Get
                Return NewPressedRKeys
            End Get
        End Property

        Friend Sub KeyEvent(ByVal WMCode As Integer, ByVal Key As Keys)
            If WMCode = WM_KEYDOWN Then
                If Not NewPressedRKeys.Contains(Key) Then
                    NewPressedRKeys.Add(Key)
                    RaiseEvent RKeyDown(Me, Key)
                End If
                RaiseEvent RKeyPressed(Me, Key)
            ElseIf WMCode = WM_KEYUP Then
                NewPressedRKeys.Remove(Key)
                RaiseEvent RKeyUp(Me, Key)
            End If
        End Sub

        Private Function ConvertTo32BPP(ByVal Original As Bitmap) As Bitmap

            Dim ConvertedBitmap As Bitmap = New Bitmap(Original.Width, Original.Height, PixelFormat.Format32bppArgb)

            Using Renderer As Graphics = Graphics.FromImage(ConvertedBitmap)
                Renderer.DrawImage(Original, New Rectangle(0, 0, Original.Width, Original.Height))
            End Using

            Return ConvertedBitmap
        End Function

        Public WriteOnly Property Image() As Bitmap
            Set(ByVal NewImage As Bitmap)
                Select Case LCDType
                    Case KeyboardLCDTtype.NoLCD
                        Throw New InvalidOperationException("Keyboard type has no LCD")
                    Case KeyboardLCDTtype.Monochrome160x43
                        If NewImage Is Nothing Then
                            HardwareInterface.WriteData(Me, FormatLCDBW160(New Bitmap(160, 43, PixelFormat.Format32bppArgb)))
                        Else
                            If NewImage.Width <> 160 OrElse NewImage.Height <> 43 Then
                                Throw New ArgumentOutOfRangeException("Image must be 160x43px")
                            End If
                            HardwareInterface.WriteData(Me, FormatLCDBW160(ConvertTo32BPP(NewImage)))
                        End If
                    Case KeyboardLCDTtype.Colour320x240
                        If NewImage Is Nothing Then
                            HardwareInterface.WriteData(Me, FormatLCDColour320(New Bitmap(320, 240, PixelFormat.Format32bppArgb)))
                        Else
                            If NewImage.Width <> 320 OrElse NewImage.Height <> 240 Then
                                Throw New ArgumentOutOfRangeException("Image must be 320x240px")
                            End If
                            HardwareInterface.WriteData(Me, FormatLCDColour320(ConvertTo32BPP(NewImage)))
                        End If
                End Select
            End Set
        End Property

        Public Property BacklightColour() As Color
            Get
                Return InterpretGetFeatureData(FeatureType.BacklightColour, HardwareInterface.GetFeature(Me, GetGetFeatureData(FeatureType.BacklightColour)))
            End Get
            Set(ByVal NewColour As Color)
                HardwareInterface.SetFeature(Me, GetSetFeatureData(FeatureType.BacklightColour, NewColour))
            End Set
        End Property

        Public Property KeyboardBacklightIntensity() As KeyboardBacklightIntensity
            Get
                Return InterpretGetFeatureData(FeatureType.BacklightIntensity, HardwareInterface.GetFeature(Me, GetGetFeatureData(FeatureType.BacklightIntensity)))
            End Get
            Set(ByVal NewIntensity As KeyboardBacklightIntensity)
                HardwareInterface.SetFeature(Me, GetSetFeatureData(FeatureType.BacklightIntensity, NewIntensity))
            End Set
        End Property

        Public WriteOnly Property LCDContrast() As LCDContrast
            Set(ByVal NewContrast As LCDContrast)
                HardwareInterface.SetFeature(Me, GetSetFeatureData(FeatureType.LCDContrast, NewContrast))
            End Set
        End Property

        Public Property LCDBacklightIntensity() As LCDBacklightIntensity
            Get
                Return InterpretGetFeatureData(FeatureType.LCDBacklightIntensity, HardwareInterface.GetFeature(Me, GetGetFeatureData(FeatureType.LCDBacklightIntensity)))
            End Get
            Set(ByVal NewIntensity As LCDBacklightIntensity)
                HardwareInterface.SetFeature(Me, GetSetFeatureData(FeatureType.LCDBacklightIntensity, NewIntensity))
            End Set
        End Property

        Public Property MLEDs() As MLEDState
            Get
                Return InterpretGetFeatureData(FeatureType.MLEDs, HardwareInterface.GetFeature(Me, GetGetFeatureData(FeatureType.MLEDs)))
            End Get
            Set(ByVal LightStates As MLEDState)
                HardwareInterface.SetFeature(Me, GetSetFeatureData(FeatureType.MLEDs, LightStates))
            End Set
        End Property

        Protected Friend Overridable Sub DeviceChangeComplete()

        End Sub
    End Class
End Namespace