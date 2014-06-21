Imports GLib
Imports GLib.Win32
Imports GLib.Enumerations.DeviceChangeMessages
Imports GLib.Win32.WindowsMessages

Friend Class frmDeviceChangeCatcher
    Friend Controller As GSeriesController

    Friend Sub New(ByVal GLibcontroller As GSeriesController)
        InitializeComponent()

        Controller = GLibcontroller
    End Sub

    Protected Overrides Sub WndProc(ByRef Message As System.Windows.Forms.Message)
        Select Case Message.Msg
            Case WM_DEVICECHANGE
                Select Case Message.WParam
                    Case DBT_DEVICEREMOVECOMPLETE, DBT_DEVICEARRIVAL
                        Dim DeviceInfo As DEV_BROADCAST_HDR = DirectCast(Marshal.PtrToStructure(Message.LParam, GetType(DEV_BROADCAST_HDR)), DEV_BROADCAST_HDR)
                        If DeviceInfo.dbch_DeviceType = DBT_DEVTYP_DEVICEINTERFACE Then
                            Controller.SignalDeviceChange()
                        End If
                    Case DBT_DEVNODES_CHANGED
                        Controller.SignalDeviceChange()
                End Select
            Case WM_INPUT
                If Controller.ProcessRawInput(Message) Then Return
        End Select
        MyBase.WndProc(Message)
    End Sub

End Class