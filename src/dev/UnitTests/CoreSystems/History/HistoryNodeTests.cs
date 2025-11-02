using Arcanum.Core.CoreSystems.History;
using Moq;

namespace UnitTests.CoreSystems.History;

[TestFixture]
public class HistoryTests
{
   [Test]
   public void HistoryNode_Constructor_InitializesCorrectly()
   {
      var cmd = new Mock<ICommand>().Object;
      var node = new HistoryNode(1, cmd, HistoryEntryType.Normal);

      Assert.That(node.Id, Is.EqualTo(1));
      Assert.That(node.Command, Is.EqualTo(cmd));
      Assert.That(node.EntryType, Is.EqualTo(HistoryEntryType.Normal));
      Assert.That(node.Children, Is.Empty);
      Assert.That(node.IsCompacted, Is.False);
      Assert.That(node.Parent, Is.Null);
   }

   [Test]
   public void HistoryNode_GetChildWithId_ReturnsCorrectChild()
   {
      var child = new HistoryNode(2, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var node = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal) { Children = [child] };

      var result = node.GetChildWithId(2);
      Assert.That(result, Is.EqualTo(child));
   }

   [Test]
   public void HistoryNode_GetChildWithId_ThrowsIfNotFound()
   {
      var node = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      Assert.Throws<InvalidOperationException>(() => node.GetChildWithId(999));
   }

   [Test]
   public void HistoryNode_Enumerator_TraversesCorrectly()
   {
      var root = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var child = new HistoryNode(2, new Mock<ICommand>().Object, HistoryEntryType.Normal) { Parent = root };
      root.Children.Add(child);

      var result = root.ToList();
      Assert.That(result.Count, Is.EqualTo(2));
      Assert.That(result[0].Node, Is.EqualTo(root));
      Assert.That(result[1].Node, Is.EqualTo(child));
      Assert.That(result[0].Level, Is.EqualTo(0));
      Assert.That(result[1].Level, Is.EqualTo(1));
   }

   [Test]
   public void CompactHistoryNode_InsertInTree_ReplacesNodesCorrectly()
   {
      var parent = new HistoryNode(0, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var n1 = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal) { Parent = parent };
      parent.Children.Add(n1);
      var n2 = new HistoryNode(2, new Mock<ICommand>().Object, HistoryEntryType.Normal) { Parent = parent };
      parent.Children.Add(n2);

      var compacted = new CompactHistoryNode(10, [n1]);
      compacted.InsertInTree();

      Assert.That(parent.Children.Contains(compacted));
      Assert.That(n1.Parent, Is.Null);
   }

   [Test]
   public void CompactHistoryNode_UnCompact_RestoresNodesCorrectly()
   {
      var parent = new HistoryNode(0, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var n1 = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal) { Parent = parent };
      parent.Children.Add(n1);

      var compacted = new CompactHistoryNode(10, [n1]);
      compacted.InsertInTree();
      compacted.UnCompact();

      Assert.That(parent.Children.Contains(n1));
      Assert.That(n1.Parent, Is.EqualTo(parent));
   }

   [Test]
   public void CompactHistoryNode_StepUndo_CallsUndoOnCorrectNode()
   {
      var cmd1 = new Mock<ICommand>();
      var cmd2 = new Mock<ICommand>();

      var n1 = new HistoryNode(1, cmd1.Object, HistoryEntryType.Normal);
      var n2 = new HistoryNode(2, cmd2.Object, HistoryEntryType.Normal);
      var compacted = new CompactHistoryNode(10, [n1, n2]);
      compacted.StepUndo();

      cmd1.Verify(c => c.Undo(), Times.Once);
   }

   [Test]
   public void CompactHistoryNode_GetStepRedoCommand_ReturnsCorrectCommand()
   {
      var cmd1 = new Mock<ICommand>();
      var cmd2 = new Mock<ICommand>();

      var n1 = new HistoryNode(1, cmd1.Object, HistoryEntryType.Normal);
      var n2 = new HistoryNode(2, cmd2.Object, HistoryEntryType.Normal);
      var compacted = new CompactHistoryNode(10, [n1, n2]);

      compacted.StepUndo();
      var cmd = compacted.GetStepRedoCommand();
      Assert.That(cmd, Is.EqualTo(n2.Command));
   }

   [Test]
   public void CompactHistoryNode_FullUndo_CallsUndoOnAllInReverse()
   {
      var cmd1 = new Mock<ICommand>();
      var cmd2 = new Mock<ICommand>();

      var n1 = new HistoryNode(1, cmd1.Object, HistoryEntryType.Normal);
      var n2 = new HistoryNode(2, cmd2.Object, HistoryEntryType.Normal);
      var compacted = new CompactHistoryNode(10, [n1, n2]);

      compacted.FullUndo();

      cmd2.Verify(c => c.Undo(), Times.Once);
      cmd1.Verify(c => c.Undo(), Times.Once);
   }

   [Test]
   public void CompactHistoryNode_FullRedo_CallsRedoOnAllInOrder()
   {
      var cmd1 = new Mock<ICommand>();
      var cmd2 = new Mock<ICommand>();

      var n1 = new HistoryNode(1, cmd1.Object, HistoryEntryType.Normal);
      var n2 = new HistoryNode(2, cmd2.Object, HistoryEntryType.Normal);
      var compacted = new CompactHistoryNode(10, [n1, n2]);

      compacted.FullRedo();

      cmd1.Verify(c => c.Redo(), Times.Once);
      cmd2.Verify(c => c.Redo(), Times.Once);
   }

   [Test]
   public void CompactHistoryNode_HasStepUndo_And_HasStepRedo_WorkCorrectly()
   {
      var n1 = new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var n2 = new HistoryNode(2, new Mock<ICommand>().Object, HistoryEntryType.Normal);
      var compacted = new CompactHistoryNode(10, [n1, n2]);

      Assert.That(compacted.HasStepUndo, Is.True);
      Assert.That(compacted.HasStepRedo, Is.False);
   }

   [Test]
   public void CompactHistoryNode_GetDescription_ReturnsCorrectString()
   {
      var compacted =
         new CompactHistoryNode(10, [new HistoryNode(1, new Mock<ICommand>().Object, HistoryEntryType.Normal)]);
      Assert.That(compacted.GetDescription, Is.EqualTo("Compacting 1 Nodes"));
   }
}