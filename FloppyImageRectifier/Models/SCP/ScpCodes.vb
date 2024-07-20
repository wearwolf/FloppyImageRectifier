Public Module ScpCodes
    Public Enum ScpDiskType As Byte
        Unknown = &HFF
        disk_C64 = &H0
        disk_Amiga = &H4
        disk_AmigaHD = &H8

        ' ATARI DISK TYPES
        disk_AtariFMSS = &H10
        disk_AtariFMDS = &H11
        disk_AtariFMEx = &H12
        disk_AtariSTSS = &H14
        disk_AtariSTDS = &H15

        ' APPLE DISK TYPES
        disk_AppleII = &H20
        disk_AppleIIPro = &H21
        disk_Apple400K = &H24
        disk_Apple800K = &H25
        disk_Apple144 = &H26

        ' PC DISK TYPES
        disk_PC360K = &H30
        disk_PC720K = &H31
        disk_PC12M = &H32
        disk_PC144M = &H33

        ' TANDY DISK TYPES
        disk_TRS80SSSD = &H40
        disk_TRS80SSDD = &H41
        disk_TRS80DSSD = &H42
        disk_TRS80DSDD = &H43

        ' TI DISK TYPES
        disk_TI994A = &H50

        ' ROLAND DISK TYPES
        disk_D20 = &H60

        ' AMSTRAD DISK TYPES
        disk_CPC = &H70

        ' OTHER DISK TYPES
        disk_360 = &H80
        disk_12M = &H81
        disk_Rrsvd1 = &H82
        disk_Rsrvd2 = &H83
        disk_720 = &H84
        disk_144M = &H85

        ' TAPE DRIVE DISK TYPES
        tape_GCR1 = &HE0
        tape_GCR2 = &HE1
        tape_MFM = &HE2

        ' HARD DRIVE DISK TYPES
        drive_MFM = &HF0
        drive_RLL = &HF1
    End Enum

    <Flags>
    Public Enum ScpImageFlags As Byte
        IndexQueued = &H1
        Tpi96 = &H2
        Rpm360 = &H4
        FluxNormalized = &H8
        ReadWrite = &H10
        ExtensionFooter = &H20
        NonFloppyMedia = &H40
        CreatedByOtherDevice = &H80
    End Enum

    Public Enum ScpHeads As Byte
        Unknown = &HFF
        Both = &H0
        Side0 = &H1
        Side1 = &H2
    End Enum

End Module
