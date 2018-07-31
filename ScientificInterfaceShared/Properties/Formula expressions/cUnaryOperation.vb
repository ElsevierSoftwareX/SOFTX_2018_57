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
    ''' Implements an expression as an unary operation, i.e. an
    ''' arithmetical computation on one numeral.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cUnaryOperation
        : Inherits cExpression

        ''' ---------------------------------------------------------------
        ''' <summary>Supported arithmetical operations.</summary>
        ''' <remarks>Extend this enumberated type with any Unary operation you need. 
        ''' Just don't forget to add the implementation in <see cref="CalcValue">CalcValue</see>.
        ''' </remarks>
        ''' ---------------------------------------------------------------
        Public Enum eOperatorType
            ''' <summary>Returns the quare root of the operand.</summary>
            Sqrt
            ''' <summary>Returns the sine of the angle specified in operand.</summary>
            Sin
            ''' <summary>Returns the inverse sine of the number [-1, 1] specified in operand.</summary>
            Asin
            ''' <summary>Returns the cosine of the angle specified in operand.</summary>
            Cos
            ''' <summary>Returns the inverse cosine of the number [-1, 1] specified in operand.</summary>
            Acos
            ''' <summary>Returns the tangent of the angle specified in operand.</summary>
            Tan
            ''' <summary>Returns the inverse tangent of the number [-1, 1] specified in operand.</summary>
            Atan
            ''' <summary>Returns operand rounded-up.</summary>
            Ceil
            ''' <summary>Returns operand rounded-down.</summary>
            Floor
            ''' <summary>Returns the operand rounded to the nearest whole number.</summary>
            Round
            ''' <summary>Returns the sign of operand.</summary>
            Sign
            ''' <summary>Returns the absolute value of operand.</summary>
            Abs
            ''' <summary>Returns the natural (base-e) logarithm of operand.</summary>
            Log
            ''' <summary>Returns the base-10 logarithm of operand.</summary>
            Log10

        End Enum

        ''' <summary>Unary operator to perform.</summary>
        Private m_nOperator As eOperatorType = 0
        ''' <summary>Operand to perform operator onto.</summary>
        Private WithEvents m_operand As cExpression = Nothing
        ''' <summary>Cached calcuated value</summary>
        Private m_sValue As Single = 0.0
        ''' <summary>Cached style</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new cUnaryOperation
        ''' </summary>
        ''' <param name="nOperator"><see cref="eOperatorType">Operator</see> to perform.</param>
        ''' <param name="operand">Operand to perform operator onto.</param>
        ''' <remarks>For supported operand types, see <see cref="cFormulaProperty.GetExpression">cFormulaProperty.GetExpression</see>.</remarks>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal nOperator As eOperatorType, ByVal operand As Object)
            Me.m_nOperator = nOperator
            Me.m_operand = cFormulaProperty.GetExpression(operand)
            Me.m_sValue = Me.CalcValue()
            Me.m_style = Me.CalcStyle()
            ' Start listening for operand changes
            AddHandler Me.m_operand.OnValueChanged, AddressOf OnOperandValueChanged
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            ' Stop listening for operand changes
            RemoveHandler Me.m_operand.OnValueChanged, AddressOf OnOperandValueChanged
            Me.m_operand.Dispose()
            Me.m_operand = Nothing
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
            Dim s1 As Single = Me.m_operand.GetValue()
            Dim s As Single = 0

            Me.m_style = Me.m_operand.GetStyle()

            Select Case Me.m_nOperator
                Case eOperatorType.Sqrt
                    s = CSng(Math.Sqrt(s1))
                Case eOperatorType.Sin
                    s = CSng(Math.Sin(s1))
                Case eOperatorType.Asin
                    s = CSng(Math.Asin(s1))
                Case eOperatorType.Cos
                    s = CSng(Math.Cos(s1))
                Case eOperatorType.Acos
                    s = CSng(Math.Acos(s1))
                Case eOperatorType.Tan
                    s = CSng(Math.Tan(s1))
                Case eOperatorType.Atan
                    s = CSng(Math.Atan(s1))
                Case eOperatorType.Ceil
                    s = CSng(Math.Ceiling(s1))
                Case eOperatorType.Floor
                    s = CSng(Math.Floor(s1))
                Case eOperatorType.Round
                    s = CSng(Math.Round(s1))
                Case eOperatorType.Sign
                    s = CSng(Math.Sign(s1))
                Case eOperatorType.Abs
                    s = CSng(Math.Abs(s1))
                Case eOperatorType.Log
                    s = CSng(Math.Log(s1))
                Case eOperatorType.Log10
                    s = CSng(Math.Log10(s1))
                Case Else
                    Debug.Assert(False, String.Format("Operator {0} not implemented", Me.m_nOperator))
            End Select
            Return s
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the style of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcStyle() As cStyleGuide.eStyleFlags
            Return Me.m_operand.GetStyle()
        End Function

    End Class

End Namespace
