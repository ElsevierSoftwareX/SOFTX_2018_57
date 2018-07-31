' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

#Region " Imports "

Option Strict On

Imports EwECore.FishingPolicy
Imports ZedGraph

#End Region ' Imports

Namespace Ecosim

    <CLSCompliant(False)> _
    Public Class cFPSKiteDiagramHelper
        Inherits cZedGraphKiteHelper

        Public Sub UpdateDiagram(ByVal fpsr As cFPSSearchResults)

            Dim lLines As New List(Of LineItem)
            Dim asValKite(3) As Single
            Dim MaxS As Single = 0.0!

            For i As Integer = 0 To 3
                asValKite(i) = fpsr.CriteriaValues(i + 1)
                If asValKite(i) > MaxS Then MaxS = CSng(Math.Ceiling(asValKite(i) - 0.5!))
                If MaxS < asValKite(i) Then MaxS += 1.0!
            Next

            lLines.Add(Me.CreateLineItem("CritValues", Color.AliceBlue, asValKite))
            Me.PlotLines(lLines.ToArray)

        End Sub

#If 0 Then
    ' Old EwE5 logic
    Public Sub MakeKiteDiagram(CritVal() As Single)
Dim i As Integer
Dim j As Integer
Dim MaxS As Integer
On Local Error GoTo exitSub
    picKite.Cls
    MaxS = 2
    For i = 1 To 4
        valKite(i) = CritVal(i)
        If CritVal(i) > MaxS Then MaxS = Int(CritVal(i) - 0.5) + 1
        If MaxS < CritVal(i) Then MaxS = MaxS + 1
    Next
    picKite.Scale (-MaxS, MaxS)-(MaxS, -MaxS)
    picKite.DrawWidth = 1
    If Label(1, 0) = 0 And Label(1, 1) = 0 Then 'labels not yet estimated
        Label(1, 0) = MaxS / 2
        Label(1, 1) = 0.5
        Label(2, 0) = 0.2
        Label(2, 1) = -MaxS / 2
        Label(3, 0) = -MaxS / 2
        Label(3, 1) = 0.5
        Label(4, 0) = 0.2
        Label(4, 1) = MaxS / 2
    End If
    If Label(1, 0) > MaxS Then Label(1, 0) = MaxS / 2
    If Label(2, 1) < -MaxS Then Label(2, 1) = -MaxS / 2
    If Label(3, 0) < -MaxS Then Label(3, 0) = -MaxS / 2
    If Label(4, 1) > MaxS Then Label(4, 1) = MaxS / 2
    picKite.CurrentX = Label(1, 0)  'MaxS / 2
    picKite.CurrentY = Label(1, 1)  '.5
    picKite.Print "Social"
    picKite.CurrentX = Label(2, 0)  '0.2
    picKite.CurrentY = Label(2, 1)  '-MaxS / 2
    picKite.Print "Ecosystem"
    picKite.CurrentX = Label(3, 0)  '-MaxS / 2
    picKite.CurrentY = Label(3, 1)  '.5
    picKite.Print "Mandated"
    picKite.CurrentX = Label(4, 0)  '0.2
    picKite.CurrentY = Label(4, 1)  'MaxS / 2
    picKite.Print "Economic"
    'For i = 1 To 4
    '    For j = 1 To CritVal(i)
    '
    '    Next
    'Next
    j = if(CritVal(3) > 0, 1, 0)
    picKite.Circle (0, 0), 0.01
    picKite.ForeColor = QBColor(2)
    picKite.Line (0, 0)-(1, 0)
    picKite.Line (0, 0)-(0, 1)
    picKite.Line (0, 0)-(0, 1)
    picKite.Line (0, 0)-(-j, 0)
    'between end of lines
    picKite.Line (0, 1)-(1, 0)
    picKite.Line (1, 0)-(0, -1)
    picKite.Line (0, -1)-(-j, 0)
    picKite.Line (-j, 0)-(0, 1)
    picKite.ForeColor = QBColor(1)
    picKite.Line (0, 0)-(CritVal(2), 0)
    picKite.Line (0, 0)-(0, -CritVal(4))
    picKite.Line (0, 0)-(-CritVal(3), 0)
    picKite.Line (0, 0)-(0, CritVal(1))
    picKite.DrawWidth = 2

    If CritVal(1) < 0 Then picKite.ForeColor = QBColor(4)
    picKite.Line (-CritVal(3), 0)-(0, CritVal(1))
    picKite.Line (0, CritVal(1))-(CritVal(2), 0)
    picKite.ForeColor = QBColor(1)
    picKite.Line (CritVal(2), 0)-(0, -CritVal(4))
    picKite.Line (0, -CritVal(4))-(-CritVal(3), 0)
exitSub:
End Sub
#End If
    End Class

End Namespace
