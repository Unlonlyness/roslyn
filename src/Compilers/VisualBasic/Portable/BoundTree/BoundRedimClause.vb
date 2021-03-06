﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    Partial Class BoundRedimClause

#If DEBUG Then
        Private Sub Validate()
            Select Case Operand.Kind
                Case BoundKind.LateInvocation
                    Dim invocation = DirectCast(Operand, BoundLateInvocation)

                    If Not invocation.ArgumentsOpt.IsDefault Then
                        For Each arg In invocation.ArgumentsOpt
                            Debug.Assert(Not arg.IsSupportingAssignment())
                        Next
                    End If
            End Select
        End Sub
#End If
    End Class

End Namespace
