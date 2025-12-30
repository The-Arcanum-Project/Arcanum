using System.Reflection;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.MapEditor.Engine.Core.Math;
using Arcanum.Core.MapEditor.Engine.Core.Spatial;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[TestFixture]
public class QuadTreeNodeTests
{
   private class TestItem : ISpatialEntity
   {
      public int Id { get; } = Guid.NewGuid().GetHashCode();
      public Vector2I Position2D { get; set; }
      public RectF Bounds { get; set; }
      public TestItem(float x, float y, float w, float h) => Bounds = new(x, y, w, h);
      public override string ToString() => $"Item({Bounds.X},{Bounds.Y})";
   }

   private QuadTree<TestItem> _tree;
   private RectF _worldBounds;

   [SetUp]
   public void Setup()
   {
      _worldBounds = new(0, 0, 100, 100);
      _tree = new(ref _worldBounds, maxObjectsPerNode: 2, maxDepth: 4);
   }

   #region Internal State Inspection Helpers (Reflection)

   private object GetRootNode()
   {
      var field = typeof(QuadTree<TestItem>).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
      return field!.GetValue(_tree)!;
   }

   private static List<TestItem> GetNodeItems(object node)
   {
      var field = node.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
      return (field!.GetValue(node) as List<TestItem>)!;
   }

   private Array? GetNodeChildren(object node)
   {
      var field = node.GetType().GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
      return field!.GetValue(node) as Array;
   }

   private object GetChild(object node, int index)
   {
      var children = GetNodeChildren(node);
      Assert.That(children, Is.Not.Null, "Node has no children (Leaf node)");
      return children.GetValue(index)!;
   }

   #endregion

   [Test]
   public void Insert_Logic_ShouldNotInitializeItemsList_UntilFirstInsert()
   {
      // Test lazy initialization of _items ??= new(maxObjects)
      var root = GetRootNode();
      var items = GetNodeItems(root);

      Assert.That(items, Is.Null, "Node._items should be null before any insertion to save memory");

      _tree.Insert(new(10, 10, 5, 5));
      items = GetNodeItems(root);

      Assert.That(items, Is.Not.Null);
      Assert.That(items, Has.Count.EqualTo(1));
   }

   [Test]
   public void GetChildIndex_Logic_VerifyQuadrants()
   {
      // We force a split by adding 3 items (max is 2)
      // World is 0,0 to 100,100. Midpoint is 50,50.

      // Top Right (>50 X, <50 Y)
      var tr = new TestItem(60, 10, 5, 5);
      // Top Left (<50 X, <50 Y)
      var tl = new TestItem(10, 10, 5, 5);
      // Bottom Left (<50 X, >50 Y)
      var bl = new TestItem(10, 60, 5, 5);

      _tree.Insert(tr);
      _tree.Insert(tl);
      _tree.Insert(bl); // Triggers split

      var root = GetRootNode();

      // Verify Root is empty (items moved down)
      Assert.That(GetNodeItems(root), Is.Empty);

      // Verify Distribution
      // Children indices: 0:TR, 1:TL, 2:BL, 3:BR
      var childTr = GetChild(root, 0);
      var childTl = GetChild(root, 1);
      var childBl = GetChild(root, 2);
      var childBr = GetChild(root, 3);

      Assert.Multiple(() =>
      {
         Assert.That(GetNodeItems(childTr), Has.Count.EqualTo(1), "Top Right should have 1 item");
         Assert.That(GetNodeItems(childTl), Has.Count.EqualTo(1), "Top Left should have 1 item");
         Assert.That(GetNodeItems(childBl), Has.Count.EqualTo(1), "Bottom Left should have 1 item");
         Assert.That(GetNodeItems(childBr), Is.Null, "Bottom Right should be uninitialized/null");
      });
   }

   [Test]
   public void Split_LoopLogic_BackwardsIteration_ShouldPreserveData()
   {
      _tree = new(ref _worldBounds, maxObjectsPerNode: 2, maxDepth: 1);

      var i1 = new TestItem(10, 10, 2, 2);
      var i2 = new TestItem(12, 12, 2, 2);
      var i3 = new TestItem(14, 14, 2, 2);

      _tree.Insert(i1);
      _tree.Insert(i2);
      _tree.Insert(i3);

      var root = GetRootNode();
      var tlChild = GetChild(root, 1);

      Assert.That(GetNodeItems(tlChild), Has.Count.EqualTo(3));
   }

   [Test]
   public void Remove_SwapRemove_Logic_ShouldNotCorruptList()
   {
      _tree = new(ref _worldBounds, maxObjectsPerNode: 10);

      var itemA = new TestItem(1, 1, 1, 1);
      var itemB = new TestItem(2, 2, 1, 1);
      var itemC = new TestItem(3, 3, 1, 1);

      _tree.Insert(itemA); // Index 0
      _tree.Insert(itemB); // Index 1
      _tree.Insert(itemC); // Index 2

      // Remove item A (Index 0). 
      // Swap-remove should take C (Index 2), put it at Index 0, and remove Index 2.
      _tree.Remove(itemA);

      var rootItems = GetNodeItems(GetRootNode());

      Assert.That(rootItems, Has.Count.EqualTo(2));
      Assert.That(rootItems, Does.Contain(itemB));
      Assert.That(rootItems, Does.Contain(itemC));
      Assert.That(rootItems, Does.Not.Contain(itemA));
   }

   [Test]
   public void QueryRange_ManualChildUnrolling_ShouldFunctionCorrectly()
   {
      _tree = new(ref _worldBounds, maxObjectsPerNode: 1); // Split immediately

      var tr = new TestItem(80, 10, 5, 5); // Quadrant 0
      var tl = new TestItem(10, 10, 5, 5); // Quadrant 1
      var bl = new TestItem(10, 80, 5, 5); // Quadrant 2
      var br = new TestItem(80, 80, 5, 5); // Quadrant 3

      _tree.Insert(tr);
      _tree.Insert(tl);
      _tree.Insert(bl);
      _tree.Insert(br);

      // Query specifically strictly inside Quadrant 3 (Bottom Right)
      var resBr = _tree.Query(new(70, 70, 20, 20));
      Assert.That(resBr, Has.Count.EqualTo(1));
      Assert.That(resBr[0].Id, Is.EqualTo(br.Id));

      // Query specifically strictly inside Quadrant 1 (Top Left)
      var resTl = _tree.Query(new(0, 0, 20, 20));
      Assert.That(resTl, Has.Count.EqualTo(1));
      Assert.That(resTl[0].Id, Is.EqualTo(tl.Id));
   }

   [Test]
   public void MidPoint_Calculations_ShouldBePrecise()
   {
      // This tests the `_midX` and `_midY` pre-calculation logic.
      // We place an item EXACTLY on the floating point boundary.
      // 50.0f

      var boundaryItem = new TestItem(50.0f, 10, 10, 10);

      // Logic check:
      // item.X (50) < _midX (50) ? FALSE
      // item.X (50) > _midX (50) ? FALSE
      // Result: Should return -1 (Straddle/Parent), even though it visually starts right on the line.

      _tree.Insert(boundaryItem); // Item 1
      _tree.Insert(new(10, 10, 2, 2)); // Item 2
      _tree.Insert(new(12, 12, 2, 2)); // Item 3 (Trigger split)

      var root = GetRootNode();
      var rootItems = GetNodeItems(root);

      // The boundary item should have stayed in root because (50 < 50) is false.
      Assert.That(rootItems, Does.Contain(boundaryItem));
   }
}