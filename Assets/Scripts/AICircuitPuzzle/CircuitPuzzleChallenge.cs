using System;
using UnityEngine;

public enum CircuitPuzzleMode
{
    BuildCircuit,
    SolveExercise
}

public enum CircuitValueSource
{
    A,
    B,
    C,
    D,
    Previous
}

[Serializable]
public class CircuitOperationStep
{
    public LogicGateType gateType = LogicGateType.AND;
    public CircuitValueSource leftInput = CircuitValueSource.A;
    public CircuitValueSource rightInput = CircuitValueSource.B;
}

[Serializable]
public class CircuitPuzzleChallenge
{
    [Header("Modo")]
    public CircuitPuzzleMode mode = CircuitPuzzleMode.BuildCircuit;

    [Header("Identificacao")]
    public string challengeName = "Novo desafio";

    [TextArea(1, 3)]
    public string circuitText;

    [Header("Entradas")]
    public bool inputA;
    public bool inputB;
    public bool inputC;
    public bool inputD;

    [Header("Operacoes")]
    public CircuitOperationStep[] steps;

    [Header("Montar Circuito")]
    public bool desiredOutput = true;

    public int RequiredSlotCount
    {
        get { return steps != null ? steps.Length : 0; }
    }

    public string GetDisplayText()
    {
        string text = challengeName + "\n";
        text += "A = " + BoolToBit(inputA) + "   B = " + BoolToBit(inputB);

        if (UsesInput(CircuitValueSource.C))
        {
            text += "   C = " + BoolToBit(inputC);
        }

        if (UsesInput(CircuitValueSource.D))
        {
            text += "   D = " + BoolToBit(inputD);
        }

        text += "\nCircuito: " + GetCircuitText();

        if (mode == CircuitPuzzleMode.BuildCircuit)
        {
            text += "\nSaida = " + BoolToBit(desiredOutput);
            text += "\nMonte os slots em ordem.";
        }
        else
        {
            text += "\nQual e a saida?";
            text += "\nOpcoes: 0 ou 1";
        }

        return text;
    }

    public string GetCircuitText()
    {
        if (!string.IsNullOrEmpty(circuitText))
        {
            return circuitText;
        }

        if (steps == null || steps.Length == 0)
        {
            return "?";
        }

        string expression = StepToText(steps[0]);
        for (int i = 1; i < steps.Length; i++)
        {
            string right = SourceToText(steps[i].rightInput);
            expression = "(" + expression + ") " + LogicGateUtility.GetGateLabel(steps[i].gateType) + " " + right;
        }

        return expression;
    }

    public bool EvaluateWithConfiguredSteps()
    {
        if (steps == null || steps.Length == 0)
        {
            return false;
        }

        bool previous = false;
        for (int i = 0; i < steps.Length; i++)
        {
            previous = EvaluateStep(steps[i], steps[i].gateType, previous);
        }

        return previous;
    }

    public bool EvaluateWithPlacedGates(LogicGateType[] placedGates)
    {
        if (steps == null || placedGates == null || placedGates.Length < steps.Length)
        {
            return false;
        }

        bool previous = false;
        for (int i = 0; i < steps.Length; i++)
        {
            previous = EvaluateStep(steps[i], placedGates[i], previous);
        }

        return previous;
    }

    private bool EvaluateStep(CircuitOperationStep step, LogicGateType gateType, bool previous)
    {
        bool left = GetValue(step.leftInput, previous);
        bool right = GetValue(step.rightInput, previous);
        return LogicGateUtility.Evaluate(gateType, left, right);
    }

    private bool GetValue(CircuitValueSource source, bool previous)
    {
        switch (source)
        {
            case CircuitValueSource.A:
                return inputA;
            case CircuitValueSource.B:
                return inputB;
            case CircuitValueSource.C:
                return inputC;
            case CircuitValueSource.D:
                return inputD;
            case CircuitValueSource.Previous:
                return previous;
            default:
                return false;
        }
    }

    private string StepToText(CircuitOperationStep step)
    {
        string left = SourceToText(step.leftInput);
        string right = SourceToText(step.rightInput);
        return left + " " + LogicGateUtility.GetGateLabel(step.gateType) + " " + right;
    }

    private string SourceToText(CircuitValueSource source)
    {
        switch (source)
        {
            case CircuitValueSource.A:
                return "A";
            case CircuitValueSource.B:
                return "B";
            case CircuitValueSource.C:
                return "C";
            case CircuitValueSource.D:
                return "D";
            case CircuitValueSource.Previous:
                return "Anterior";
            default:
                return "?";
        }
    }

    private bool UsesInput(CircuitValueSource source)
    {
        if (steps == null)
        {
            return false;
        }

        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i] != null && (steps[i].leftInput == source || steps[i].rightInput == source))
            {
                return true;
            }
        }

        return false;
    }

    private string BoolToBit(bool value)
    {
        return value ? "1" : "0";
    }
}
