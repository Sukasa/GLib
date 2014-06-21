Imports System.Windows.Forms

Namespace GLib
    Public Class G15v2Keyboard
        Inherits GSeriesKeyboard

        Friend Sub New()

        End Sub

        Friend Overrides Sub SpecificInit()
            Dim B() As Byte = New Byte(991) {}
            B(0) = 2
            HardwareInterface.WriteData(Me, B)
        End Sub

        Public Overrides ReadOnly Property LCDType() As Enumerations.KeyboardLCDTtype
            Get
                Return KeyboardLCDTtype.Monochrome160x43
            End Get
        End Property

        Friend Overrides Function GetSetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureValue As Object) As Byte()
            Dim FeatureBytes() As Byte = {}
            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightIntensity
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 2
                    FeatureBytes(1) = 1
                    FeatureBytes(2) = FeatureValue

                Case Enumerations.FeatureType.LCDContrast
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 2
                    FeatureBytes(1) = 32
                    FeatureBytes(2) = 129
                    FeatureBytes(3) = FeatureValue

                Case Enumerations.FeatureType.LCDBacklightIntensity
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 2
                    FeatureBytes(1) = 2
                    FeatureBytes(2) = FeatureValue

                Case Enumerations.FeatureType.MLEDs
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 2
                    FeatureBytes(1) = 4
                    FeatureBytes(2) = (Not FeatureValue) And MLEDState.All
            End Select
            Return FeatureBytes
        End Function

        Friend Overrides Function InterpretGetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureData() As Byte) As Object
            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightIntensity
                    Return FeatureData(2)

                Case Enumerations.FeatureType.LCDBacklightIntensity
                    Return FeatureData(2)

                Case Enumerations.FeatureType.MLEDs
                    Return (Not FeatureData(3)) And 15
            End Select
            Return Nothing
        End Function

        Friend Overrides Function GetGetFeatureData(ByVal FeatureType As Enumerations.FeatureType) As Byte()
            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightIntensity
                    Return New Byte() {2, 1, 0, 0}

                Case Enumerations.FeatureType.LCDBacklightIntensity
                    Return New Byte() {2, 2, 0, 0}

                Case Enumerations.FeatureType.MLEDs
                    Return New Byte() {2, 4, 0, 0}
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
            Return PressedGKeys
        End Function

        Friend Overrides Function DecodeMKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.MKeys)
            Dim PressedMKeys As New List(Of MKeys)()

            If (Code(1) And 64) = 64 Then
                PressedMKeys.Add(MKeys.M1)
            End If
            If (Code(1) And 128) = 128 Then
                PressedMKeys.Add(MKeys.M2)
            End If
            If (Code(2) And 32) = 32 Then
                PressedMKeys.Add(MKeys.M3)
            End If
            If (Code(2) And 64) = 64 Then
                PressedMKeys.Add(MKeys.MR)
            End If

            Return PressedMKeys
        End Function

        Friend Overrides Function DecodeSKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.SKeys)
            Dim PressedSKeys As New List(Of SKeys)()

            If (Code(2) And 128) = 128 Then
                PressedSKeys.Add(SKeys.SoftSwap)
            End If
            If (Code(2) And 2) = 2 Then
                PressedSKeys.Add(SKeys.Soft0)
            End If
            If (Code(2) And 4) = 4 Then
                PressedSKeys.Add(SKeys.Soft1)
            End If
            If (Code(2) And 8) = 8 Then
                PressedSKeys.Add(SKeys.Soft2)
            End If
            If (Code(2) And 16) = 16 Then
                PressedSKeys.Add(SKeys.Soft3)
            End If
            If (Code(2) And 1) = 1 Then
                PressedSKeys.Add(SKeys.SoftBacklight)
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

        Friend Overrides Sub DispatchKeyboardUpdate(ByVal Code() As Byte)
            Select Case Code(0)

                Case 1 ' Media Keys
                    ProcessMediaKeysStatusChange(Code)

                Case 2 ' Special Keys
                    ProcessSpecialKeysStatusChange(Code)

            End Select
        End Sub
    End Class
End Namespace