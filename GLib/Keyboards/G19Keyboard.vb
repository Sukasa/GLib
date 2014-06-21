Imports System.Windows.Forms

Namespace GLib
    Public Class G19Keyboard
        Inherits GSeriesKeyboard

        Friend Sub New()

        End Sub

        Friend Overrides Function DecodeAKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of System.Windows.Forms.Keys)

        End Function

        Friend Overrides Function DecodeGKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.GKeys)

        End Function

        Friend Overrides Function DecodeMKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.MKeys)

        End Function

        Friend Overrides Function DecodeSKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.SKeys)

        End Function

        Friend Overrides Sub DispatchKeyboardUpdate(ByVal Code() As Byte)

        End Sub

        Friend Overrides Function GetGetFeatureData(ByVal FeatureType As Enumerations.FeatureType) As Byte()

            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightColour
                    Return New Byte() {7, 0, 0, 0}

                Case Enumerations.FeatureType.MLEDs
                    Return New Byte() {5, 0}

                Case Enumerations.FeatureType.BacklightIntensity
                    Return New Byte() {}

            End Select

            Return Nothing
        End Function

        Friend Overrides Function GetSetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureValue As Object) As Byte()

            Dim FeatureBytes() As Byte = {}

            Select Case FeatureType
                Case Enumerations.FeatureType.BacklightColour
                    ReDim FeatureBytes(3)
                    FeatureBytes(0) = 7
                    Dim C As Color = CType(FeatureValue, Color)
                    FeatureBytes(1) = C.R
                    FeatureBytes(2) = C.G
                    FeatureBytes(3) = C.B

                Case Enumerations.FeatureType.LCDBrightness
                    ReDim FeatureBytes(0)
                    FeatureBytes(0) = Math.Min(Math.Max(0, FeatureValue), &H7F)

                Case Enumerations.FeatureType.MLEDs
                    ReDim FeatureBytes(2)
                    FeatureBytes(0) = 5
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

        Public Overrides ReadOnly Property LCDType() As Enumerations.KeyboardLCDTtype
            Get
                Return KeyboardLCDTtype.Colour320x240
            End Get
        End Property

        Friend Overrides Sub SpecificInit()

        End Sub

    End Class
End Namespace