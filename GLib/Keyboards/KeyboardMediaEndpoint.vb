Imports System.Windows.Forms

Namespace GLib
    Friend Class KeyboardMediaEndpoint
        Inherits GSeriesKeyboard

        'Specialty class used to capture media key endpoints, because windows does not pass keyboard information in WM_INPUT for media keys.
        'Most functionality of GSeriesKeyboard is disabled, and ideally the class and any instantiations are never exposed to client code.

        Private Master As GSeriesKeyboard

        Friend Overrides Function DecodeGKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.GKeys)
            Return New List(Of GKeys)
        End Function

        Friend Overrides Function DecodeMKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.MKeys)
            Return New List(Of MKeys)
        End Function

        Friend Overrides Function DecodeSKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of Enumerations.SKeys)
            Return New List(Of SKeys)
        End Function

        Friend Overrides Function DecodeAKeys(ByVal Code() As Byte) As System.Collections.Generic.List(Of System.Windows.Forms.Keys)
            Return New List(Of Keys)
        End Function

        Friend Overrides Function GetGetFeatureData(ByVal FeatureType As Enumerations.FeatureType) As Byte()
            Return Nothing
        End Function

        Friend Overrides Function GetSetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureValue As Object) As Byte()
            Return Nothing
        End Function

        Friend Overrides Function InterpretGetFeatureData(ByVal FeatureType As Enumerations.FeatureType, ByVal FeatureData() As Byte) As Object
            Return Nothing
        End Function

        Friend Overrides Sub SpecificInit()

        End Sub

        Friend Overrides Sub DispatchKeyboardUpdate(ByVal Code() As Byte)
            If Master IsNot Nothing Then
                Master.DispatchKeyboardUpdate(Code)
            End If
        End Sub

        Protected Friend Overrides Sub DeviceChangeComplete()
            Master = MultiPartResolver.Instance.GetAssociatedKeyboard(DevicePath)
        End Sub

        Friend Sub New()

        End Sub
    End Class

End Namespace