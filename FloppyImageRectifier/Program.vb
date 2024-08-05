Imports System
Imports System.IO
Imports System.Text

Module Program
    Sub Main(args As String())
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

        If args.Length < 1 OrElse args.Length > 10 Then
            Console.WriteLine($"Unexpected number of arguments {args.Length}")
            Ussage()
            Return
        End If

        Dim scpFilePath = String.Empty
        Dim hfeFilePath = String.Empty
        Dim imgFilePath = String.Empty
        Dim diskType = FloppyDiskType.Unknown
        Dim outputPath = String.Empty
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
                Case "-output"
                    If args.Length <= i + 1 Then
                        Console.WriteLine("missing value for output argument")
                        Ussage()
                        Exit Sub
                    Else
                        outputPath = args(i + 1)
                        i = i + 1
                    End If
            End Select
        Next

        Using outputWriter = New OutputWriter(outputPath)
            If Not String.IsNullOrEmpty(scpFilePath) AndAlso (Not String.IsNullOrEmpty(hfeFilePath) OrElse Not String.IsNullOrEmpty(imgFilePath)) Then
                ConvertScp(scpFilePath, hfeFilePath, imgFilePath, diskType, outputWriter)
            ElseIf Not String.IsNullOrEmpty(hfeFilePath) AndAlso Not String.IsNullOrEmpty(imgFilePath) Then
                ConvertHfe(hfeFilePath, imgFilePath, diskType, outputWriter)
            ElseIf Not String.IsNullOrEmpty(scpFilePath) Then
                DisplayScp(scpFilePath, outputWriter)
            ElseIf Not String.IsNullOrEmpty(hfeFilePath) Then
                DisplayHfe(hfeFilePath, outputWriter)
            ElseIf Not String.IsNullOrEmpty(imgFilePath) Then
                DisplayImg(imgFilePath, outputWriter)
            Else
                Console.WriteLine("Missing required argument")
                Ussage()
            End If
        End Using
    End Sub

    Sub ConvertScp(scpFilePath As String, hfeFilePath As String, imgFilePath As String, diskType As FloppyDiskType, outputWriter As OutputWriter)
        If diskType = FloppyDiskType.Unknown Then
            Console.WriteLine("Missing disk type argument")
            Ussage()
            Exit Sub
        End If

        If Not File.Exists(scpFilePath) Then
            Console.WriteLine($"Unable to find SCP file: {scpFilePath}")
            Return
        End If

        Console.WriteLine($"Reading SCP file: {scpFilePath}")
        Dim scpFile = New ScpFile(scpFilePath)
        scpFile.Read()

        Console.WriteLine($"Decoding SCP file")
        Dim scpDecoder = New ScpDecoder(scpFile)
        Dim mfmFile = scpDecoder.DecodeMfm(diskType)
        mfmFile.CheckChecksums(outputWriter)

        Console.WriteLine($"Writing SCP file: {scpFilePath}")
        scpFile.UpdateDiskType(diskType)
        scpFile.Write()

        Dim writeHfe = True
        If Not String.IsNullOrEmpty(hfeFilePath) Then
            If File.Exists(hfeFilePath) Then
                writeHfe = False
                Console.WriteLine($"HFE file '{hfeFilePath}' already exists, do you want to overwrite it? (y/n)")

                Dim result = Console.ReadLine().First()
                If result = "y"c OrElse result = "Y"c Then
                    writeHfe = True
                End If
            End If

            If writeHfe Then
                Console.WriteLine($"Writing HFE file: {hfeFilePath}")
                Dim hfeFile = New HfeFile(hfeFilePath)
                Dim hfeEncoder = New HfeEncoder(hfeFile, mfmFile)
                hfeEncoder.Encode()
                hfeFile.Write()
            End If
        End If

        Dim writeImg = True
        If Not String.IsNullOrEmpty(imgFilePath) Then
            If File.Exists(imgFilePath) Then
                writeImg = False
                Console.WriteLine($"IMG file '{imgFilePath}' already exists, do you want to overwrite it? (y/n)")

                Dim result = Console.ReadLine().First()
                If result = "y"c OrElse result = "Y"c Then
                    writeImg = True
                End If
            End If

            If writeImg Then
                Console.WriteLine($"Writing IMG file: {imgFilePath}")
                Dim imgFile = New ImgFile(imgFilePath)
                Dim imgEncoder = New ImgEncoder(imgFile, mfmFile)
                imgEncoder.Encode()
                imgFile.Write()
            End If
        End If
    End Sub

    Sub ConvertHfe(hfeFilePath As String, imgFilePath As String, diskType As FloppyDiskType, outputWriter As OutputWriter)
        If diskType = FloppyDiskType.Unknown Then
            Console.WriteLine("Missing disk type argument")
            Ussage()
            Exit Sub
        End If

        If Not File.Exists(hfeFilePath) Then
            Console.WriteLine($"Unable to find HFE file: {hfeFilePath}")
            Return
        End If

        Console.WriteLine($"Reading HFE file: {hfeFilePath}")
        Dim hfeFile = New HfeFile(hfeFilePath)
        hfeFile.Read()

        Console.WriteLine($"Decoding HFE file")
        Dim hfeDecoder = New HfeDecoder(hfeFile)
        Dim mfmFile = hfeDecoder.DecodeMfm(diskType)
        mfmFile.CheckChecksums(outputWriter)

        If File.Exists(imgFilePath) Then
            Console.WriteLine($"IMG file '{imgFilePath}' already exists, do you want to overwrite it? (y/n)")

            Dim result = Console.Read()
            If result <> AscW("y"c) AndAlso result <> AscW("Y"c) Then
                Return
            End If
        End If

        Console.WriteLine($"Writing IMG file: {imgFilePath}")
        Dim imgFile = New ImgFile(imgFilePath)
        Dim imgEncoder = New ImgEncoder(imgFile, mfmFile)
        imgEncoder.Encode()
        imgFile.Write()
    End Sub

    Sub DisplayScp(scpFilePath As String, outputWriter As OutputWriter)
        If Not File.Exists(scpFilePath) Then
            Console.WriteLine($"Unable to find SCP file: {scpFilePath}")
            Return
        End If

        Console.WriteLine($"Reading SCP file: {scpFilePath}")
        Dim scpFile = New ScpFile(scpFilePath)
        scpFile.Read()
        scpFile.WriteOutput(outputWriter)
    End Sub

    Sub DisplayHfe(hfeFilePath As String, outputWriter As OutputWriter)
        If Not File.Exists(hfeFilePath) Then
            Console.WriteLine($"Unable to find HFE file: {hfeFilePath}")
            Return
        End If

        Console.WriteLine($"Reading HFE file: {hfeFilePath}")
        Dim hfeFile = New HfeFile(hfeFilePath)
        hfeFile.Read()
        hfeFile.WriteOutput(outputWriter)
    End Sub

    Sub DisplayImg(imgFilePath As String, outputWriter As OutputWriter)
        If Not File.Exists(imgFilePath) Then
            Console.WriteLine($"Unable to find IMG file: {imgFilePath}")
            Return
        End If

        Console.WriteLine($"Reading IMG file: {imgFilePath}")
        Dim imgFile = New ImgFile(imgFilePath)
        imgFile.Read()
        imgFile.WriteOutput(outputWriter)
    End Sub

    Sub Ussage()
        Console.WriteLine("FloppyImageRectifier.exe -scp <Path-to-SCP-File> -hfe <Path-to-HFE-File> -img <Path-to-IMG-File> -type <disk-Type-Identifier> [-output <path-to-file>]")
        Console.WriteLine("<Path-to-SCP-File> - A path to an SCP file, will always be an input file if provided, may be an output file")
        Console.WriteLine("<Path-to-HFE-File> - A path to an HFE file, may be input Or output depending on other options provided")
        Console.WriteLine("<Path-to-IMG-file> - A path to an IMG file, may be input Or output depending on other options provided")
        Console.WriteLine("<disk-Type-Identifier> - Defines the type of floppy disk")
        Console.WriteLine("<path-to-file> - Optional, path to a file where some output will be mirrored")
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
