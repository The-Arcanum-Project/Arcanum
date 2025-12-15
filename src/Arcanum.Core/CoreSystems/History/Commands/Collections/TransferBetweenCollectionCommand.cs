using System.Diagnostics;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using static Arcanum.Core.CoreSystems.Selection.SelectionHelpers;
namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class TransferBetweenLinksCommand : Eu5ObjectCommand
{

    private readonly record struct ObjectData(IEu5Object Source, IEu5Object Value);
    
    private readonly IEu5Object _target;
    
    private IList<ObjectData> _targets = new List<ObjectData>();
    private readonly Enum _attribute;
    
    private readonly Enum _parentAttribute;
    
    public TransferBetweenLinksCommand(IEu5Object target, Enum attribute, IEu5Object value) : base(target, attribute)
    {
        _target = target;
        _attribute = attribute;
        _parentAttribute = GetParentEnumFromChildrenEnum(_attribute);

        var addition = new ObjectData((IEu5Object)value._getValue(_parentAttribute), value);
        
        _targets.Add(addition);
        
        addition.Source._removeFromCollection(_attribute, addition.Value);
        _target._addToCollection(_attribute, addition.Value);
        InvalidateTargets(addition.Source);
    }

    public TransferBetweenLinksCommand(IEu5Object target, Enum attribute, IEnumerable<IEu5Object> value) : base(target, attribute)
    {
        _target = target;
        _attribute = attribute;
        _parentAttribute = GetParentEnumFromChildrenEnum(_attribute);

        foreach (var val in value)
        {
            var addition = new ObjectData((IEu5Object)val._getValue(_parentAttribute), val);
            _targets.Add(addition);
            
            addition.Source._removeFromCollection(_attribute, addition.Value);
            _target._addToCollection(_attribute, addition.Value);
            InvalidateTargets(addition.Source, false);
        }
        
        InvalidateUI();
        
        FinalizeSetup();
    }
    


    public override string GetDescription => _targets.Count == 1
        ? $"Transferred {_targets[0].Value} to {_target}"
        : $"Transferred {_targets.Count} {_attribute} to {_target}";
    public override IEu5Object[] GetTargets() => _targets.Select(x => x.Source).Append(_target).ToArray();

    public sealed override void FinalizeSetup()
    {
        base.FinalizeSetup();
        _targets = _targets.ToArray();
    }
    
    public override void Redo()
    {
        foreach (var target in _targets)
        {
            target.Value._setValue(_parentAttribute, _target);
        }
        base.Redo();
    }

    public override void Undo()
    {
        foreach (var target in _targets)
        {
            target.Value._setValue(_parentAttribute, target.Source);
        }
        base.Undo();
    }
    
    public bool TryAdd(IEu5Object target, Enum attribute, IEu5Object value)
    {
        if (DisallowMerge(target, attribute))
            return false;

        var addition = new ObjectData((IEu5Object)value._getValue(_parentAttribute), value);
        _targets.Add(addition);
        
        addition.Source._removeFromCollection(_attribute, addition.Value);
        _target._addToCollection(_attribute, addition.Value);

        InvalidateTargets(target, false);
        InvalidateTargets(addition.Source);
        return true;
    }
    
    public bool TryAdd(IEu5Object target, Enum attribute, IEnumerable<IEu5Object> value)
    {
        if (DisallowMerge(target, attribute))
            return false;

        foreach (var val in value)
        {
            var addition = new ObjectData((IEu5Object)val._getValue(_parentAttribute), val);
            _targets.Add(addition);
            
            addition.Source._removeFromCollection(_attribute, addition.Value);
            _target._addToCollection(_attribute, addition.Value);
            InvalidateTargets(addition.Source, false);
        }

        InvalidateTargets(target);

        return true;
    }
    
}

public class RemoveFromLinkCommand : Eu5ObjectCommand
{

    
    private readonly IEu5Object _target;
    private readonly IEu5Object _empty;
    
    private IList<IEu5Object> _values = new List<IEu5Object>();
    private readonly Enum _attribute;
    
    private readonly Enum _parentAttribute;
    
    public RemoveFromLinkCommand(IEu5Object target, Enum attribute, IEu5Object value) : base(target, attribute)
    {
        _target = target;
        _empty = (IEu5Object)EmptyRegistry.Empties[target.GetType()];
        _attribute = attribute;
        _parentAttribute = GetParentEnumFromChildrenEnum(_attribute);

        
        _values.Add(value);
        
        _target._removeFromCollection(_attribute, value);
        InvalidateUI();
    }

    public RemoveFromLinkCommand(IEu5Object target, Enum attribute, IEu5Object[] value) : base(target, attribute)
    {
        _target = target;
        _empty = (IEu5Object)EmptyRegistry.Empties[target.GetType()];
        _attribute = attribute;
        _parentAttribute = GetParentEnumFromChildrenEnum(_attribute);

        _values = value;
        
        foreach (var val in value) _target._removeFromCollection(_attribute, val);
        
        base.FinalizeSetup();
        InvalidateUI();
    }
    


    public override string GetDescription => _values.Count == 1
        ? $"Removed {_values[0]} from {_target}"
        : $"Removed {_values.Count} {_attribute} from {_target}";
    public override IEu5Object[] GetTargets() => [_target];

    public override void FinalizeSetup()
    {
        base.FinalizeSetup();
        _values = _values.ToArray();
    }
    
    public override void Redo()
    {
        foreach (var target in _values)
        {
            target._setValue(_parentAttribute, _empty);
        }
        base.Redo();
    }

    public override void Undo()
    {
        foreach (var target in _values)
        {
            target._setValue(_parentAttribute, _target);
        }
        base.Undo();
    }
    
    public bool TryAdd(IEu5Object target, Enum attribute, IEu5Object value)
    {
        if (DisallowMerge(target, attribute) || !target.Equals(_target))
            return false;

        _values.Add(value);
        
        _target._removeFromCollection(_attribute, value);
        
        InvalidateTargets(value);

        return true;
    }
    
    public bool TryAdd(IEu5Object target, Enum attribute, IEnumerable<IEu5Object> value)
    {
        if (DisallowMerge(target, attribute))
            return false;
        
        var valueArray = value.ToArray();
        
        foreach (var val in valueArray)
        {
            _values.Add(val);
            _target._removeFromCollection(_attribute, val);
        }

        InvalidateTargets(valueArray);

        return true;
    }
    
}