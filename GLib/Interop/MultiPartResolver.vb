Imports Microsoft.Win32.SafeHandles

Namespace GLib

    Friend Class MultiPartResolver

        Private KnownPartDefs As New List(Of Part)
        Private PathsToKeyboards As New List(Of Part)

        Private Shared _Instance As New MultiPartResolver

        Private Sub New()

            'Don't forget to add the necessary paths to HardwareInterface.vb (@ bottom of file)

            KnownPartDefs.Add(New Part("vid_046d&pid_C22d&mi_00", 2)) '      G510, Audio disabled
            KnownPartDefs.Add(New Part("vid_046d&pid_c22d&mi_01&col01", 2)) '..Media Keys
            KnownPartDefs.Add(New Part("vid_046d&pid_c22d&mi_01&col02", 2)) '..RawInput Keyboard

            KnownPartDefs.Add(New Part("vid_046d&pid_C22e&mi_00", 2)) '      G510, Audio enabled
            KnownPartDefs.Add(New Part("vid_046d&pid_c22e&mi_01&col01", 2)) '..Media Keys
            KnownPartDefs.Add(New Part("vid_046d&pid_c22e&mi_01&col02", 2)) '..RawInput Keyboard

            KnownPartDefs.Add(New Part("vid_046d&pid_c222&col02", 2)) '      G15v1 (Untested)
            KnownPartDefs.Add(New Part("vid_046d&pid_c227&col02", 2)) '      G15v2
            KnownPartDefs.Add(New Part("vid_046d&pid_c226&mi_01&col01", 3)) '..Media Keys
            KnownPartDefs.Add(New Part("vid_046d&pid_c226&mi_00", 3)) '      ..RawInput Keyboard

        End Sub

        Public Function GetAssociatedKeyboard(ByVal DevicePath As String) As GSeriesKeyboard
            DevicePath = DevicePath.Substring(0, DevicePath.IndexOf("{"))
            Dim Part As Part = PathsToKeyboards.Find(Function(T) T.SearchPath.ToLower = DevicePath.ToLower)
            If Part Is Nothing Then Return Nothing
            Return Part.Node.AssociatedKeyboard
        End Function

        Public Shared Function Instance() As MultiPartResolver
            Return _Instance
        End Function

        Public Sub ClearPartsList()
            PathsToKeyboards.Clear()
            CommonNode.ClearNodeList()
        End Sub

        Public Sub AddPath(ByVal DevicePath As String, ByVal DeviceHandle As UInteger)
            Dim Part As Part = KnownPartDefs.Find(Function(T) DevicePath.ToLower.Contains(T.DevicePath.ToLower))
            If Part Is Nothing Then Return
            Part = Part.Create(DeviceHandle, DevicePath)
            PathsToKeyboards.Add(Part)
        End Sub

        Public Sub RegisterConnectedKeyboard(ByVal Keyboard As GSeriesKeyboard)
            Dim Part As Part = PathsToKeyboards.Find(Function(T) T.DevicePath.ToLower = Keyboard.DevicePath.ToLower)
            If Part IsNot Nothing Then Part.Node.AssociatedKeyboard = Keyboard
        End Sub

        Friend Class CommonNode
            Private Shared AllCommonNodes As New List(Of CommonNode)

            Public NodePath As String
            Public AssociatedKeyboard As GSeriesKeyboard

            Friend Shared Sub ClearNodeList()
                AllCommonNodes.Clear()
            End Sub

            Public Shared Function GetNode(ByVal NodeDevicePath As String) As CommonNode
                Dim Node As CommonNode = AllCommonNodes.Find(Function(T) T.NodePath.ToLower = NodeDevicePath.ToLower)
                If Node Is Nothing Then
                    Node = New CommonNode(NodeDevicePath)
                End If
                Return Node
            End Function

            Private Sub New(ByVal NodeDevicePath As String)
                NodePath = NodeDevicePath
                AllCommonNodes.Add(Me)
            End Sub

        End Class

        Friend Class Part

            Public DevicePath As String
            Public SearchPath As String
            Public StepsUpToCommon As Integer
            Public Node As CommonNode

            Private Sub New()
            End Sub

            Public Sub New(ByVal Path As String, ByVal Steps As Integer)
                StepsUpToCommon = Steps
                DevicePath = Path
            End Sub

            Public Function Create(ByVal Handle As UInteger, ByVal WorkingPath As String) As Part
                Dim NewPart As New Part
                NewPart.DevicePath = WorkingPath
                NewPart.SearchPath = WorkingPath.Substring(0, WorkingPath.IndexOf("{"))

                Dim NodeDeviceID As Integer = Handle

                For X As Integer = 1 To StepsUpToCommon
                    Dim NewNodeDeviceID As UInteger = HardwareInterface.GetParentDeviceInstanceHandle(NodeDeviceID)
                    If NodeDeviceID <> Handle Then HardwareInterface.CloseInterface(NodeDeviceID)
                    NodeDeviceID = NewNodeDeviceID
                Next

                NewPart.Node = CommonNode.GetNode(HardwareInterface.GetDevicePathFromInstance(NodeDeviceID))
                HardwareInterface.CloseInterface(NodeDeviceID)

                Return NewPart
            End Function

        End Class

    End Class

End Namespace