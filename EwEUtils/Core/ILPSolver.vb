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


#End Region ' Imports

Namespace Core


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Template for linear programming engines.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The methods in this interface are based on the Microsoft Solver Foundation
    ''' API (http://msdn.microsoft.com/en-us/library/ff713957%28v=vs.93%29.aspx).
    ''' </para>
    ''' <para>
    ''' MSF is not referenced in the project because of it's distribution model.
    ''' Via this interface any LP engine can be connected to EwE.
    ''' </para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Interface ILPSolver

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a variable to the LP solver.
        ''' </summary>
        ''' <param name="key">Object to identify the variable. Not used in 
        ''' computations, just added for interface compliance.</param>
        ''' <param name="iVar">Key that the LP solver assigns to the variable.
        ''' Note that this key is not an array index; all rows and variables
        ''' share an incrementing ID which is used in <see cref="GetValue"/>
        ''' to get values for rows and variables.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddVariable(ByVal key As Object, ByRef iVar As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a row to the LP solver.
        ''' </summary>
        ''' <param name="key">Object to identify the row. Not used in 
        ''' computations, just added for interface compliance.</param>
        ''' <param name="iRow">Key that the LP solver assigns to the row.
        ''' Note that this key is not an array index; all rows and variables
        ''' share an incrementing ID which is used in <see cref="GetValue"/>
        ''' to get values for rows and variables.</param>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddRow(ByVal key As Object, ByRef iRow As Integer) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set the bounds for a variable.
        ''' </summary>
        ''' <param name="iVar">Variable to set the bounds for.</param>
        ''' <param name="dMin">Lower bound for the variable, or <see cref="Double.NegativeInfinity"/> 
        ''' if no lower bound applies.</param>
        ''' <param name="dMax">Upper bound for the variable, or <see cref="Double.NegativeInfinity"/> 
        ''' if no upper bound applies.</param>
        ''' -------------------------------------------------------------------
        Sub SetBounds(ByVal iVar As Integer, ByVal dMin As Double, ByVal dMax As Double)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Set a coefficient in the LP solver matrix.
        ''' </summary>
        ''' <param name="iRow">Row key to set the coefficient for.</param>
        ''' <param name="iVar">Variable key to set the coefficient for.</param>
        ''' <param name="dVal">Coefficient to set for row and variable.</param>
        ''' -------------------------------------------------------------------
        Sub SetCoefficient(ByVal iRow As Integer, ByVal iVar As Integer, ByVal dVal As Double)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add an optimization goal to the LP solver.
        ''' </summary>
        ''' <param name="iRow">Key of the row that defines the goal.</param>
        ''' <param name="iPriority">Goal priority.</param>
        ''' <param name="bMinimize">Flag, stating whether the solver should
        ''' minimize (True) or maximize (False) the goals.</param>
        ''' <remarks>Note that LP engines may support only a single goal, and 
        ''' may not accept priorities.</remarks>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function AddGoal(ByVal iRow As Integer, ByVal iPriority As Integer, ByVal bMinimize As Boolean) As Boolean

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Run the LP solver.
        ''' </summary>
        ''' <returns>True if successful.</returns>
        ''' -------------------------------------------------------------------
        Function Solve(ByVal iTimeStepIndex As Integer) As Core.eSolverReturnValues

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract the optimized value for a row or a variable.
        ''' </summary>
        ''' <param name="iItem">The key to a row or variable to obtain the 
        ''' optimized value for.</param>
        ''' <returns>The computed optimized value for the row or variable.</returns>
        ''' -------------------------------------------------------------------
        Function GetValue(ByVal iItem As Integer) As Double


        Function GetDualValue(ByVal iItem As Integer) As Double

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns whether this solver is supported by the operating system.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Function IsSupported() As Boolean

    End Interface

End Namespace
