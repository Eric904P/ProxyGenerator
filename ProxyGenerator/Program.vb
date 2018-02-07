Imports System.IO
Imports System.Windows.Forms
Imports ProxyGenerator.My.Resources

'Public Class Program
Module Program
    ReadOnly Scraper As New Scraper
    ReadOnly Checker As New Checker

    'Save dialogue
    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = v2_Threading_SaveFile_txt_files____txt____txt
            fs.FilterIndex = 1
            fs.ShowDialog()
            If Not (fs.FileName = Nothing) Then
                Using sw As New StreamWriter(fs.FileName)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            Console.WriteLine(v2_Threading_SaveFile_Please_wait_until_program_is_finished_)
        End If
    End Sub

    'Deletes files on close
    Public Declare Auto Function SetConsoleCtrlHandler Lib "kernel32.dll"(Handler As HandlerRoutine, add As Boolean) _
        As Boolean

    Public Delegate Function HandlerRoutine(CtrlType As CtrlTypes) As Boolean

    Public Enum CtrlTypes
        CTRL_C_EVENT = 0
        CTRL_BREAK_EVENT
        CTRL_CLOSE_EVENT
        CTRL_LOGOFF_EVENT = 5
        CTRL_SHUTDOWN_EVENT
    End Enum

    'Handles window close event
    Public Function ControlHandler(ctrlType As CtrlTypes) As Boolean
        If ctrlType = CtrlTypes.CTRL_CLOSE_EVENT Then
            Dim FileList As String() = Directory.GetFiles(Path.GetTempPath())
            For Each f as String in FileList
                If f.Contains("ProxyGen") Then
                    File.Delete(f)
                    Console.WriteLine("DELETED" + f)
                End If
            Next
            Return True
        End If
        Return False
    End Function

    'main function
    Sub Main()
        SetConsoleCtrlHandler(New HandlerRoutine(AddressOf ControlHandler), True)

        Console.WindowWidth = 81
        Console.SetCursorPosition(left := 0, top := 0)
        Console.WriteLine(Mainheader)
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        'Console.WriteLine("/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ PROXY GENERATOR v2.5 /\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine(" /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ BY ERIC904P /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        'Console.WriteLine("- - - - - - - - - - - - - - - -PRESS ENTER TO START- - - - - - - - - - - - - - -")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.ReadKey()

        If Scraper.ScrapeHerder() Then
            If Checker.CheckHerder() Then
                SaveFile(Checker.LoadChecked(true))
            End If
        End If
    End Sub
End Module
'End Class


