using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class SetValueCommand : ICommand
{
    private readonly record struct ObjectData(IEu5Object Target, object Value);
    
    private IList<ObjectData> _targets = new List<ObjectData>();
    private readonly Enum _attribute;
    private readonly Type _type;
    private bool _initialized;
    private readonly object _value;
    public IEu5Object[] GetTargets() => _targets.Select(x => x.Target).ToArray();

    public string GetDescription => _targets.Count > 1 ?
        $"Set {_attribute} to {_value} on {_targets.Count} objects of type {_type}" : 
        $"Set {_attribute} to {_value} on {_targets.First().Target}";
    
    public SetValueCommand(IEu5Object target, Enum attribute, object value)
    {
        _type = target.GetType();
        _value = value;
        _targets.Add(new (target, target._getValue(attribute)));
        _attribute = attribute;
        SaveMaster.InitCommand(this, target);
    }

    public void FinalizeSetup()
    {
        _targets = _targets.ToArray();
        _initialized = true;
    }

    public void Execute()
    {
        SaveMaster.CommandExecuted(this);
    }

    public void Undo()
    {
        SaveMaster.CommandUndone(this);
        foreach(var target in _targets)
            target.Target._setValue(_attribute, target.Value);
    }

    public void Redo()
    {
        SaveMaster.CommandExecuted(this);
        foreach(var target in _targets)
            target.Target._setValue(_attribute, _value);
    }

    public List<int> GetTargetHash()
    {
        return _targets.Select(t => t.GetHashCode()).ToList();
    }

    public bool TryAdd(IEu5Object target, Enum attribute, object value)
    {
        if(target.GetType() != _type || !attribute.Equals(_attribute) || !value.Equals(_value) || _initialized)
            return false;
        _targets.Add(new (target,value));
        SaveMaster.AddToCommand(this, target);
        return true;
    }

    public string GetDebugInformation(int indent)
    {
        var indentStr = new string(' ', indent);
        return $"{indentStr}DummyChangeCommand targeting {_targets.Count} objects.";
    }
}