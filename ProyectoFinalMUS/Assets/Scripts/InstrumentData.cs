using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Instrument Data", menuName = "Instrument Data")]
public class InstrumentData : ScriptableObject
{
    public string instrumentName;
    [Header("Value of each Button")]
    [TextArea]
    public string ButtonIndexes = "0 -> RIGHT\n1 -> LEFT\n2 -> UP\n3 -> DOWN\n4 -> Y\n5 -> X\n6 -> A\n7 -> B\n";
    public List<float> values;
}