Namespace GLib
    Public Module Enumerations

        Public Enum LCDContrast As Byte
            High = 26
            Medium = 22
            Low = 18
        End Enum

        Public Enum KeyboardBacklightIntensity As Byte
            High = 2
            Low = 1
            [Off] = 0
        End Enum

        Public Enum LCDBacklightIntensity As Byte
            High = 2
            Low = 1
            [Off] = 0
        End Enum

        <Flags()> Public Enum MLEDState As Byte
            M1 = 1
            M2 = 2
            M3 = 4
            MR = 8
            None = 0
            All = M1 Or M2 Or M3 Or MR
        End Enum

        Public Enum GKeys As Integer
            G1
            G2
            G3
            G4
            G5
            G6
            G7
            G8
            G9
            G10
            G11
            G12
            G13
            G14
            G15
            G16
            G17
            G18
        End Enum

        Public Enum SKeys As Integer
            Soft0
            Soft1
            Soft2
            Soft3
            Soft4
            SoftSwap
            SoftBacklight
            SoftKeylock
            AudioMuteOutput
            AudioMuteInput
        End Enum

        Friend Enum FeatureType As Integer
            BacklightColour
            BacklightIntensity
            LCDContrast
            LCDBrightness
            LCDBacklightIntensity
            MLEDs
        End Enum

        Friend Enum DeviceChangeMessages As Integer
            DBT_CONFIGCHANGECANCELED = &H19
            DBT_CONFIGCHANGED = &H18
            DBT_CUSTOMEVENT = &H8006
            DBT_DEVICEARRIVAL = &H8000
            DBT_DEVICEQUERYREMOVE = &H8001
            DBT_DEVICEQUERYREMOVEFAILED = &H8002
            DBT_DEVICEREMOVECOMPLETE = &H8004
            DBT_DEVICEREMOVEPENDING = &H8003
            DBT_DEVICETYPESPECIFIC = &H8005
            DBT_DEVNODES_CHANGED = &H7
            DBT_QUERYCHANGECONFIG = &H17
            DBT_USERDEFINED = &HFFFF
        End Enum

        Public Enum MKeys
            M1
            M2
            M3
            MR
        End Enum

        Public Enum KeyboardLCDTtype
            NoLCD
            Monochrome160x43
            Colour320x240
        End Enum
    End Module
End Namespace