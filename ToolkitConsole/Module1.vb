﻿Imports System.IO
Imports System.Reflection

Module Module1

    Sub PrintUsage()
        Console.WriteLine("Usage: ToolkitConsole.exe <source> <destination> [hans raw file name] [-source-cxi] [-key0|-hans]")
        Console.WriteLine("<source> can either be a decrypted CCI/3DS ROM, or a directory created by ToolkitConsole.exe")
        Console.WriteLine("<destination> can be a *.3DS, *.3DZ, *.CCI, or *.CIA file, a directory if the source is a ROM, or the root of your SD card if outputting files for HANS.")
        Console.WriteLine("[hans raw file name] is the future name of the raw files for HANS, if the ""-hans"" argument is present.  Shorter strings work better, but the exact requirements are unknown.")
        Console.WriteLine("Output format is detected by the extension.  *.CIA files are outputted as CIA files, *.3DZ files are outputted as 0-key encrypted CCI ROMs, all others are outputted as decrypted CCI ROMs.  Use the -key0 flag to output as a 0-key encrypted CCI ROM instead.")
        Console.WriteLine("")
        Console.WriteLine("Examples:")
        Console.WriteLine("Extract a CCI: ToolkitConsole.exe MyRom.3ds MyFiles")
        Console.WriteLine("Build a CIA: ToolkitConsole.exe MyFiles MyRom.cia")
        Console.WriteLine("Build files for HANS: ToolkitConsole.exe MyFiles G:/ MyHack")
    End Sub

    Sub Main()
        Try
            Console.WriteLine(".Net 3DS Toolkit v{0}", Assembly.GetExecutingAssembly.GetName.Version.ToString(3))

            Dim args = Environment.GetCommandLineArgs
            If args.Length < 3 Then
                PrintUsage()
            Else
                Dim key0 As Boolean = args.Contains("-key0")
                Dim hans As Boolean = args.Contains("-hans")
                Dim source As String = args(1)
                Dim destination As String = args(2)

                If Not Path.IsPathRooted(source) Then
                    source = Path.Combine(Environment.CurrentDirectory, source)
                End If

                If Not Path.IsPathRooted(destination) Then
                    destination = Path.Combine(Environment.CurrentDirectory, destination)
                End If

                If File.Exists(source) Then

                    'Extraction mode
                    Using c As New DotNet3dsToolkit.Converter
                        Console.WriteLine("Extracting to ""{0}""...", destination)

                        If Path.GetExtension(source).ToLower = ".cxi" OrElse args.Contains("-source-cxi") Then
                            c.ExtractCXI(source, destination).Wait()
                        Else
                            c.ExtractCCI(source, destination).Wait()
                        End If

                        Console.WriteLine("Extraction complete!")
                    End Using

                ElseIf Directory.Exists(source) Then
                    'Building mode

                    Using c As New DotNet3dsToolkit.Converter
                        If hans Then
                            If args.Length > 4 Then
                                Console.WriteLine("Building files for HANS...")
                                c.BuildHans(source, destination, args(3)).Wait()
                            Else
                                Console.WriteLine("Invalid usage.")
                                PrintUsage()
                            End If
                        ElseIf key0 Then
                            Console.WriteLine("Building as 0-key encrypted CCI...")
                            c.Build3DS0Key(source, destination)
                        ElseIf Path.GetExtension(destination).ToLower = ".cia" Then
                            Console.WriteLine("Building as CIA...")
                            c.BuildCia(source, destination)
                        ElseIf Path.GetExtension(destination).ToLower = ".3dz" Then
                            Console.WriteLine("Building as 0-key encrypted CCI...")
                            c.Build3DS0Key(source, destination)
                        Else
                            Console.WriteLine("Building as decrypted CCI...")
                            c.Build3DSDecrypted(source, destination)
                        End If

                        Console.WriteLine("Build complete!")
                    End Using

                Else
                    Console.WriteLine("Error: The given source is neither a file nor a directory.")
                    Console.WriteLine("Source: ""{0}""", source)
                End If

            End If
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub

End Module
