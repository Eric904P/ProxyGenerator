Imports System
Imports System.IO
Imports System.Collections
Imports System.Linq.Expressions
Imports System.Text

Module Module1

        Public Sub Main()
            ProcessDirectory(My.Resources.Dir)
        End Sub


    ' Process all files in the directory passed in, recurse on any directories 
    ' that are found, and process the files they contain.
    Public Sub ProcessDirectory(targetDirectory As String)
        Dim fileEntries As String() = Directory.GetFiles(targetDirectory)
        ' Process the list of files found in the directory.
        Dim fileName As String
        For Each fileName In fileEntries
            ProcessFile(fileName)

        Next fileName
        Dim subdirectoryEntries As String() = Directory.GetDirectories(targetDirectory)
        ' Recurse into subdirectories of this directory.
        Dim subdirectory As String
        For Each subdirectory In subdirectoryEntries
            ProcessDirectory(subdirectory)
        Next subdirectory

    End Sub 'ProcessDirectory

    ' Insert logic for processing found files here.
    Public Sub ProcessFile(path As String)
        Dim sr As StreamReader = New StreamReader(path)
        Dim newPath = path.TrimEnd(".ini") + ".txt"
        Dim FileStr As New IO.StreamWriter(newPath, False, Encoding.GetEncoding("Windows-1252"))
        Do While Not sr.EndOfStream
            FileStr.WriteLine(sr.ReadToEnd())
        Loop
        FileStr.Close()
        sr.Close()
        Try
            File.Move(newPath, My.Resources.Dir + "Output" + newPath.Substring(newPath.LastIndexOf("/")))
        Catch ex As Exception

        End Try
        'Using(New StreamWriter(path + ".txt", false, Encoding.GetEncoding("Windows-1252")))
        '    Dim sr As StreamReader = new StreamReader(path)
        '    Dim line As String = sr.ReadToEnd()
        '    WriteLine(line)
        '    sr.Dispose()
        'End Using
    End Sub 'ProcessFile


End Module
