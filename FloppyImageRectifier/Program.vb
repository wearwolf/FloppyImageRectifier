Imports System

Module Program
    Sub Main(args As String())
        If args.Length < 1 OrElse args.Length > 8 Then
            Ussage()
            Return
        End If

        Dim scpFilePath = String.Empty
        Dim hfeFilePath = String.Empty
        Dim imgFilePath = String.Empty
        Dim diskType = FloppyDiskType.Unknown
        For i = 0 To args.Length - 1
            Select Case args(i)
                Case "-scp"
                    If args.Length <= i + 1 Then
                        Console.WriteLine("missing value for scp argument")
                        Ussage()
                        Exit Sub
                    Else
                        scpFilePath = args(i + 1)
                        i = i + 1
                    End If
                Case "-hfe"
                    If args.Length <= i + 1 Then
                        Console.WriteLine("missing value for hfe argument")
                        Ussage()
                        Exit Sub
                    Else
                        hfeFilePath = args(i + 1)
                        i = i + 1
                    End If
                Case "-img"
                    If args.Length <= i + 1 Then
                        Console.WriteLine("missing value for img argument")
                        Ussage()
                        Exit Sub
                    Else
                        imgFilePath = args(i + 1)
                        i = i + 1
                    End If
                Case "-type"
                    If args.Length <= i + 1 Then
                        Console.WriteLine("missing value for type argument")
                        Ussage()
                        Exit Sub
                    Else
                        Dim type = args(i + 1)
                        If Not [Enum].TryParse(type, diskType) Then
                            Console.WriteLine($"Unable to decode disk type '{type}'")
                            Ussage()
                            Exit Sub
                        End If
                        i = i + 1
                    End If
            End Select
        Next

        If Not String.IsNullOrEmpty(scpFilePath) AndAlso (Not String.IsNullOrEmpty(hfeFilePath) OrElse Not String.IsNullOrEmpty(imgFilePath)) Then
            ConvertScp(scpFilePath, hfeFilePath, imgFilePath, diskType)
        ElseIf Not String.IsNullOrEmpty(hfeFilePath) AndAlso Not String.IsNullOrEmpty(imgFilePath) Then
            ConvertHfe(hfeFilePath, imgFilePath, diskType)
        ElseIf Not String.IsNullOrEmpty(scpFilePath) Then
            DisplayScp(scpFilePath)
        ElseIf Not String.IsNullOrEmpty(hfeFilePath) Then
            DisplayHfe(hfeFilePath)
        ElseIf Not String.IsNullOrEmpty(imgFilePath) Then
            DisplayImg(imgFilePath)
        Else
            Console.WriteLine("Missing required argument")
            Ussage()
        End If

    End Sub

    Sub ConvertScp(scpFilePath As String, hfeFilePath As String, imgFilePath As String, diskType As FloppyDiskType)
        If diskType = FloppyDiskType.Unknown Then
            Console.WriteLine("Missing disk type argument")
            Ussage()
            Exit Sub
        End If

        Console.WriteLine("Converting SCP to HFE and/or IMG")
    End Sub

    Sub ConvertHfe(hfeFilePath As String, imgFilePath As String, diskType As FloppyDiskType)
        If diskType = FloppyDiskType.Unknown Then
            Console.WriteLine("Missing disk type argument")
            Ussage()
            Exit Sub
        End If

        Console.WriteLine("Converting HFE to IMG")
    End Sub

    Sub DisplayScp(scpFilePath As String)
        Console.WriteLine("Displaying SCP information")
    End Sub

    Sub DisplayHfe(hfeFilePath As String)
        Console.WriteLine("Displaying HFE information")
    End Sub

    Sub DisplayImg(imgFilePath As String)
        Console.WriteLine("Displaying IMG information")
    End Sub

    Sub Ussage()
        Console.WriteLine("FloppyImageRectifier.exe -scp <Path-to-SCP-File> -hfe <Path-to-HFE-File> -img <Path-to-IMG-File> -type <diskTypeIdentifier>")
        Console.WriteLine("<Path-to-SCP-File> - A path to an SCP file, will always be an input file if provided, may be an output file")
        Console.WriteLine("<Path-to-HFE-File> - A path to an HFE file, may be input Or output depending on other options provided")
        Console.WriteLine("<Path-to-IMG-file> - A path to an IMG file, may be input Or output depending on other options provided")
        Console.WriteLine("<diskTypeIdentifier> - Defines the type of floppy disk")
        Console.WriteLine("'PC_MFM_525_360' 5.25"" 320k/360k MFM encoded disk for an IBM PC")
        Console.WriteLine("'PC_MFM_525_1200' 5.25"" 1200k disk MFM encoded for an IBM PC")
        Console.WriteLine("'PC_MFM_35_720' 3.5"" 720k disk MFM encoded for an IBM PC")
        Console.WriteLine("'PC_MFM_35_1440' 3.5"" 1440k disk MFM encoded for an IBM PC")
        Console.WriteLine("'PC_MFM_35_1680' 3.5"" 1680k (DMF) disk MFM encoded for an IBM PC")
        Console.WriteLine("Include scp path, type and hfe and/or img path to convert from scp to hfe or img")
        Console.WriteLine("Include hfe path, type and img path to convert from hfe to img")
        Console.WriteLine("Include only scp, hfe or img path to display information about that file")
    End Sub
End Module
