// using Arcanum.Core.CoreSystems.SavingSystem;
// using Arcanum.Core.CoreSystems.SavingSystem.AGS;
// using Arcanum.Core.GameObjects.BaseTypes;
//
// namespace Arcanum.Core.CoreSystems.History.Commands;
//
// public class DummyChangeCommand : ICommand
// {
//    private IEu5Object[] _targets = [];
//    private List<IEu5Object> _targetsInitial = [];
//    private readonly Enum _attribute;
//    private readonly Type _type;
//
//    public string GetDescription => _targetsInitial == null!
//                                       ? $"DummyChangeCommand targeting {_targets.Length} objects with attribute {_attribute}"
//                                       : $"Waiting for more objects. Current: {_targetsInitial.Count} with attribute {_attribute}";
//
//    public DummyChangeCommand(IEu5Object target, Enum attribute)
//    {
//       _type = target.GetType();
//       _targetsInitial.Add(target);
//       _attribute = attribute;
//    }
//
//    public void FinalizeSetup()
//    {
//       _targets = _targetsInitial.ToArray();
//       _targetsInitial = null!;
//    }
//
//    public void Execute()
//    {
//       SaveMaster.CommandExecuted(this);
//    }
//
//    public void Undo()
//    {
//       SaveMaster.CommandUndone(this);
//    }
//
//    public void Redo()
//    {
//       SaveMaster.CommandExecuted(this);
//    }
//
//    public List<int> GetTargetHash()
//    {
//       return _targets.Select(t => t.GetHashCode()).ToList();
//    }
//
//    public IEu5Object[] GetTargets() => _targets;
//
//    public bool TryAdd(IEu5Object target, Enum attribute)
//    {
//       if (target.GetType() != _type || !attribute.Equals(_attribute))
//          return false;
//
//       _targetsInitial.Add(target);
//       return true;
//    }
//
//    public string GetDebugInformation(int indent)
//    {
//       var indentStr = new string(' ', indent);
//       return $"{indentStr}DummyChangeCommand targeting {_targets.Length} objects.";
//    }
// }

