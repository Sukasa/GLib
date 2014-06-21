Imports System.Windows.Forms

Namespace GLib
    Public Class G510Keyboard
        Inherits GSeriesKeyboard

        Friend Sub New()

        End Sub

        Public Overrides ReadOnly Property LCDType() As Enumerations.KeyboardLCDTtype
            Get
                Return KeyboardLCDTtype.Monochrome160x43
            End Get
        End Property

        Friend Overrides Sub DispatchKeyboardUpdate(ByVal Code() As Byte)
            Select Case Code(0)

                Case 2 ' Media Keys
                    ProcessMediaKeysStatusChange(Code)

                Case 3 ' Special Keys
                    ProcessSpecialKeysStatusChange(Code)

            End Select
        End Sub

        Friend Overrides Sub SpecificInit()
            Dim B(18) As Byte
            B(0) = 1
            HardwareInterface.SetFeature(Me, B)
        End Sub

        Friend Overrides Function GetSetFeatureData(ByVal FeatureType As FeatureType, ByVal FeatureValue As Object) As Byte()
            Dim FeatureBytes() As Byte = {}

            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightColour
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 5
                    Dim C As Color = CType(FeatureValue, Color)
                    FeatureBytes(1) = C.R
                    FeatureBytes(2) = C.G
                    FeatureBytes(3) = C.B

                Case Enumerations.FeatureType.MLEDs
                    ReDim FeatureBytes(1)
                    FeatureBytes(0) = 4
                    Dim Value As Byte = FeatureValue
                    If (Value And MLEDState.M1) <> 0 Then
                        FeatureBytes(1) += 1 << 7
                    End If
                    If (Value And MLEDState.M2) <> 0 Then
                        FeatureBytes(1) += 1 << 6
                    End If
                    If (Value And MLEDState.M3) <> 0 Then
                        FeatureBytes(1) += 1 << 5
                    End If
                    If (Value And MLEDState.MR) <> 0 Then
                        FeatureBytes(1) += 1 << 4
                    End If
            End Select
            Return FeatureBytes
        End Function

        Friend Overrides Function InterpretGetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureData() As Byte) As Object
            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightColour
                    Return Color.FromArgb(FeatureData(1), FeatureData(2), FeatureData(3))

                Case Enumerations.FeatureType.MLEDs
                    Dim MLEDs As MLEDState = MLEDState.None

                    If (FeatureData(1) And 128) <> 0 Then
                        MLEDs += MLEDState.M1
                    End If
                    If (FeatureData(1) And 64) <> 0 Then
                        MLEDs += MLEDState.M2
                    End If
                    If (FeatureData(1) And 32) <> 0 Then
                        MLEDs += MLEDState.M3
                    End If
                    If (FeatureData(1) And 16) <> 0 Then
                        MLEDs += MLEDState.MR
                    End If

                    Return MLEDs
            End Select
            Return Nothing
        End Function

        Friend Overrides Function GetGetFeatureData(ByVal FeatureType As Enumerations.FeatureType) As Byte()
            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightColour
                    Return New Byte() {5, 0, 0, 0}

                Case Enumerations.FeatureType.MLEDs
                    Return New Byte() {4, 0}
            End Select
            Return New Byte() {}
        End Function

        Friend Overrides Function DecodeGKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.GKeys)
            Dim PressedGKeys As New List(Of GKeys)()

            If (Code(1) And 1) = 1 Then
                PressedGKeys.Add(GKeys.G1)
            End If
            If (Code(1) And 2) = 2 Then
                PressedGKeys.Add(GKeys.G2)
            End If
            If (Code(1) And 4) = 4 Then
                PressedGKeys.Add(GKeys.G3)
            End If
            If (Code(1) And 8) = 8 Then
                PressedGKeys.Add(GKeys.G4)
            End If
            If (Code(1) And 16) = 16 Then
                PressedGKeys.Add(GKeys.G5)
            End If
            If (Code(1) And 32) = 32 Then
                PressedGKeys.Add(GKeys.G6)
            End If
            If (Code(1) And 64) = 64 Then
                PressedGKeys.Add(GKeys.G7)
            End If
            If (Code(1) And 128) = 128 Then
                PressedGKeys.Add(GKeys.G8)
            End If
            If (Code(2) And 1) = 1 Then
                PressedGKeys.Add(GKeys.G9)
            End If
            If (Code(2) And 2) = 2 Then
                PressedGKeys.Add(GKeys.G10)
            End If
            If (Code(2) And 4) = 4 Then
                PressedGKeys.Add(GKeys.G11)
            End If
            If (Code(2) And 8) = 8 Then
                PressedGKeys.Add(GKeys.G12)
            End If
            If (Code(2) And 16) = 16 Then
                PressedGKeys.Add(GKeys.G13)
            End If
            If (Code(2) And 32) = 32 Then
                PressedGKeys.Add(GKeys.G14)
            End If
            If (Code(2) And 64) = 64 Then
                PressedGKeys.Add(GKeys.G15)
            End If
            If (Code(2) And 128) = 128 Then
                PressedGKeys.Add(GKeys.G16)
            End If
            If (Code(3) And 1) = 1 Then
                PressedGKeys.Add(GKeys.G17)
            End If
            If (Code(3) And 2) = 2 Then
                PressedGKeys.Add(GKeys.G18)
            End If

            Return PressedGKeys
        End Function

        Friend Overrides Function DecodeMKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.MKeys)
            Dim PressedMKeys As New List(Of MKeys)()

            If (Code(3) And 16) = 16 Then
                PressedMKeys.Add(MKeys.M1)
            End If
            If (Code(3) And 32) = 32 Then
                PressedMKeys.Add(MKeys.M2)
            End If
            If (Code(3) And 64) = 64 Then
                PressedMKeys.Add(MKeys.M3)
            End If
            If (Code(3) And 128) = 128 Then
                PressedMKeys.Add(MKeys.MR)
            End If

            Return PressedMKeys
        End Function

        Friend Overrides Function DecodeSKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.SKeys)
            Dim PressedSKeys As New List(Of SKeys)()

            If (Code(4) And 1) = 1 Then
                PressedSKeys.Add(SKeys.SoftSwap)
            End If
            If (Code(4) And 2) = 2 Then
                PressedSKeys.Add(SKeys.Soft0)
            End If
            If (Code(4) And 4) = 4 Then
                PressedSKeys.Add(SKeys.Soft1)
            End If
            If (Code(4) And 8) = 8 Then
                PressedSKeys.Add(SKeys.Soft2)
            End If
            If (Code(4) And 16) = 16 Then
                PressedSKeys.Add(SKeys.Soft3)
            End If
            If (Code(3) And 4) = 4 Then
                PressedSKeys.Add(SKeys.SoftKeylock)
            End If
            If (Code(3) And 8) = 8 Then
                PressedSKeys.Add(SKeys.SoftBacklight)
            End If
            If (Code(4) And 32) = 32 Then
                PressedSKeys.Add(SKeys.AudioMuteOutput)
            End If
            If (Code(4) And 64) = 64 Then
                PressedSKeys.Add(SKeys.AudioMuteInput)
            End If

            Return PressedSKeys
        End Function

        Friend Overrides Function DecodeAKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of System.Windows.Forms.Keys)
            Dim PressedKeys As New List(Of Keys)

            If (Code(1) And 8) = 8 Then
                PressedKeys.Add(Keys.MediaPlayPause)
            End If

            If (Code(1) And 4) = 4 Then
                PressedKeys.Add(Keys.MediaStop)
            End If

            If (Code(1) And 2) = 2 Then
                PressedKeys.Add(Keys.MediaPreviousTrack)
            End If

            If (Code(1) And 1) = 1 Then
                PressedKeys.Add(Keys.MediaNextTrack)
            End If

            If (Code(1) And 16) = 16 Then
                PressedKeys.Add(Keys.VolumeMute)
            End If

            If (Code(1) And 32) = 32 Then
                PressedKeys.Add(Keys.VolumeUp)
            End If

            If (Code(1) And 64) = 64 Then
                PressedKeys.Add(Keys.VolumeDown)
            End If

            Return PressedKeys
        End Function
    End Class
End Namespace