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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Implements an expression as a K-ary operation, i.e. an
    ''' arithmetical computation on any K number of parameters where K >= 1.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cMultiOperation
        : Inherits cExpression

        ''' ---------------------------------------------------------------
        ''' <summary>Supported arithmetical operations.</summary>
        ''' ---------------------------------------------------------------
        Public Enum eOperatorType
            ''' <summary>Sums all operands.</summary>
            Sum
            ''' <summary>Multiplies all operands.</summary>
            Multiply
            ''' <summary>Returns the larger of all operands.</summary>
            Max
            ''' <summary>Returns the lower of all operands.</summary>
            Min
            ''' <summary>Returns the average of all operands.</summary>
            Avg
            ''' <summary>Returns the average of all operands that are not 0.</summary>
            AvgNonZero
        End Enum

        ''' <summary>Operator to perform</summary>
        Private m_nOperator As eOperatorType = 0
        ''' <summary>Operands</summary>
        Private m_lOperands As New List(Of cExpression)
        ''' <summary>Cached value</summary>
        Private m_sValue As Single = 0.0
        ''' <summary>Cached style</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        Private Delegate Sub OnChanged()

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new cMultiOperation. 
        ''' </summary>
        ''' <param name="nOperator"><see cref="eOperatorType">Operator</see> to perform.</param>
        ''' <param name="aOperands">Array of operands.</param>
        ''' <remarks>For supported operand types, see <see cref="cFormulaProperty.GetExpression">cFormulaProperty.GetExpression</see>.</remarks>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal nOperator As eOperatorType, ByVal aOperands() As Object)
            ' Store operator
            Me.m_nOperator = nOperator
            ' For each operand
            For nOperand As Integer = 0 To aOperands.Length - 1
                ' Resolve expression
                Dim operand As cExpression = cFormulaProperty.GetExpression(aOperands(nOperand))
                ' Add to private list of operands
                Me.m_lOperands.Add(operand)
                ' Listen to event
                AddHandler operand.OnValueChanged, AddressOf Me.OnOperandValueChanged
            Next
            ' Update value
            Me.m_sValue = Me.CalcValue()
            ' Update style
            Me.m_style = Me.CalcStyle()
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            ' For each operand
            For nOperand As Integer = 0 To Me.m_lOperands.Count - 1
                ' Get it
                Dim operand As cExpression = Me.m_lOperands(nOperand)
                ' Stop listening to its events
                RemoveHandler operand.OnValueChanged, AddressOf OnOperandValueChanged
                operand.Dispose()
            Next nOperand
            Me.m_lOperands.Clear()
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Destructor
        ''' </summary>
        ''' ---------------------------------------------------------------
        Protected Overrides Sub Finalize()
            ' For each operand
            For nOperand As Integer = 0 To Me.m_lOperands.Count - 1
                ' Get it
                Dim operand As cExpression = Me.m_lOperands(nOperand)
                ' Stop listening to its events
                RemoveHandler operand.OnValueChanged, AddressOf OnOperandValueChanged
            Next
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetValue() As Single
            Return Me.m_sValue
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="cStyleGuide.eStyleFlags">style</see>
        ''' of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetStyle() As cStyleGuide.eStyleFlags
            Return Me.m_style
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Event handler, responds to operand change events by recalculating
        ''' the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Sub OnOperandValueChanged(ByVal exp As cExpression)
            ' Calc new value and style
            Dim sVal As Single = Me.CalcValue()
            Dim style As cStyleGuide.eStyleFlags = Me.CalcStyle()
            ' Changes?
            If ((sVal <> Me.m_sValue) Or (Me.m_style <> style)) Then
                ' #Yes: set new value and style
                Me.m_sValue = sVal
                Me.m_style = style

                ' Broadcast change notification
                FireChangeNotification()
            End If
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcValue() As Single
            Dim operand As cExpression = Nothing
            Dim iHitCount As Integer = 0
            Dim s As Single = 0.0

            For nOperand As Integer = 0 To Me.m_lOperands.Count - 1
                ' Get operand
                operand = Me.m_lOperands(nOperand)
                ' Apply operator
                Select Case Me.m_nOperator
                    Case eOperatorType.Sum
                        s += operand.GetValue()
                    Case eOperatorType.Multiply
                        If nOperand = 0 Then s = operand.GetValue() Else s *= operand.GetValue()
                    Case eOperatorType.Max
                        If nOperand = 0 Then s = operand.GetValue() Else s = CSng(Math.Max(s, operand.GetValue()))
                    Case eOperatorType.Min
                        If nOperand = 0 Then s = operand.GetValue() Else s = CSng(Math.Min(s, operand.GetValue()))
                    Case eOperatorType.Avg
                        s += operand.GetValue() : iHitCount += 1
                    Case eOperatorType.AvgNonZero
                        If operand.GetValue() <> 0.0 Then s += operand.GetValue() : iHitCount += 1
                    Case Else
                        Debug.Assert(False, String.Format("Operator {0} not implemented", Me.m_nOperator))
                End Select
            Next

            ' Post-process
            Select Case Me.m_nOperator
                Case eOperatorType.Avg, eOperatorType.AvgNonZero
                    ' JS 15jan08: prevent crash
                    If (iHitCount <> 0) Then s /= iHitCount
            End Select

            Return s
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Combine the style of all operands.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcStyle() As cStyleGuide.eStyleFlags

            Dim operand As cExpression = Nothing
            Dim s As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

            For nOperand As Integer = 0 To Me.m_lOperands.Count - 1
                ' Get operand
                operand = Me.m_lOperands(nOperand)
                ' Is this the first operand?
                If nOperand = 0 Then
                    ' #Yes: copy operand style
                    s = operand.GetStyle()
                Else
                    ' #No: combine with operand style
                    s = s Or operand.GetStyle()
                End If
            Next
            Return s

        End Function

    End Class

End Namespace
