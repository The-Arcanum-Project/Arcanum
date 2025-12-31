using System.Numerics;
using System.Reflection;
using Arcanum.Core.MapEditor.Engine.Core.Math;
using Arcanum.Core.MapEditor.Engine.Core.Spatial;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[TestFixture]
public class QuadTreeNodeTests
{
   private class TestItem : I3DEntity
   {
      public int Id { get; } = Guid.NewGuid().GetHashCode();
      public Vector3 Position3D { get; set; }
      public Quaternion Rotation3D { get; set; }
      public Vector3 Scale3D { get; set; }
      public Vector3 LocalSize3D { get; set; }
      public BoundingBoxF Bounds3D { get; private set; }

      public TestItem(float x, float z, float w, float h)
      {
         var min = new Vector3(x, 0, z);
         var max = new Vector3(x + w, 10, z + h);
         Bounds3D = new(min, max);
      }

      public override string ToString() => $"Item({Bounds3D.Min.X},{Bounds3D.Min.Z})";
   }

   private QuadTree<TestItem> _tree;
   private RectF _worldBounds;

   [SetUp]
   public void Setup()
   {
      _worldBounds = new(0, 0, 100, 100);
      _tree = new(ref _worldBounds, maxObjectsPerNode: 2, maxDepth: 4);
   }

   #region Reflection Helpers

   private object GetRootNode()
   {
      var field = typeof(QuadTree<TestItem>).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
      return field!.GetValue(_tree)!;
   }

   private static List<TestItem>? GetNodeItems(object node)
   {
      var field = node.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
      return field!.GetValue(node) as List<TestItem>;
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
      return children!.GetValue(index)!;
   }

   #endregion

   [Test]
   public void Insert_Logic_ShouldNotInitializeItemsList_UntilFirstInsert()
   {
      var root = GetRootNode();
      var items = GetNodeItems(root);

      Assert.That(items, Is.Null, "Node._items should be null before any insertion");

      _tree.Insert(new(10, 10, 5, 5));
      items = GetNodeItems(root);

      Assert.That(items, Is.Not.Null);
      Assert.That(items, Has.Count.EqualTo(1));
   }

   [Test]
   public void GetChildIndex_Logic_VerifyQuadrants()
   {
      // Force split (Max 2 items)
      // Top Right (>50 X, <50 Z)
      var tr = new TestItem(60, 10, 5, 5);
      // Top Left (<50 X, <50 Z)
      var tl = new TestItem(10, 10, 5, 5);
      // Bottom Left (<50 X, >50 Z)
      var bl = new TestItem(10, 60, 5, 5);

      _tree.Insert(tr);
      _tree.Insert(tl);
      _tree.Insert(bl); // Split happens here

      var root = GetRootNode();

      // Verify Root items moved down
      Assert.That(GetNodeItems(root), Is.Empty);

      // Verify Distribution (Indices: 0:TR, 1:TL, 2:BL, 3:BR)
      var childTr = GetChild(root, 0);
      var childTl = GetChild(root, 1);
      var childBl = GetChild(root, 2);

      Assert.Multiple(() =>
      {
         Assert.That(GetNodeItems(childTr), Has.Count.EqualTo(1), "Top Right (Index 0)");
         Assert.That(GetNodeItems(childTl), Has.Count.EqualTo(1), "Top Left (Index 1)");
         Assert.That(GetNodeItems(childBl), Has.Count.EqualTo(1), "Bottom Left (Index 2)");

         // Bottom Right should exist but be empty/null items
         var childBr = GetChild(root, 3);
         Assert.That(GetNodeItems(childBr), Is.Null.Or.Empty);
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
      var tlChild = GetChild(root, 1); // Top Left

      Assert.That(GetNodeItems(tlChild), Has.Count.EqualTo(3));
   }

   [Test]
   public void Remove_SwapRemove_Logic_ShouldNotCorruptList()
   {
      // Large maxObjects to prevent splitting
      _tree = new(ref _worldBounds, maxObjectsPerNode: 10);

      var itemA = new TestItem(1, 1, 1, 1);
      var itemB = new TestItem(2, 2, 1, 1);
      var itemC = new TestItem(3, 3, 1, 1);

      _tree.Insert(itemA);
      _tree.Insert(itemB);
      _tree.Insert(itemC);

      // Remove A. Logic should swap C into A's spot.
      _tree.Remove(itemA);

      var rootItems = GetNodeItems(GetRootNode());

      Assert.That(rootItems, Has.Count.EqualTo(2));
      Assert.Multiple(() =>
      {
         // Use Id based check or ref check since we use new instances
         Assert.That(rootItems!.Any(x => x.Id == itemB.Id), Is.True);
         Assert.That(rootItems!.Any(x => x.Id == itemC.Id), Is.True);
         Assert.That(rootItems!.Any(x => x.Id == itemA.Id), Is.False);
      });
   }

   [Test]
   public void QueryRange_ManualChildUnrolling_ShouldFunctionCorrectly()
   {
      // Force immediate split
      _tree = new(ref _worldBounds, maxObjectsPerNode: 1);

      var tr = new TestItem(80, 10, 5, 5); // Index 0
      var tl = new TestItem(10, 10, 5, 5); // Index 1
      var bl = new TestItem(10, 80, 5, 5); // Index 2
      var br = new TestItem(80, 80, 5, 5); // Index 3

      _tree.Insert(tr);
      _tree.Insert(tl);
      _tree.Insert(bl);
      _tree.Insert(br);

      // Test specific quadrants to ensure loop unrolling (c[0], c[1]...) is correct
      // Bottom Right Query
      var resBr = _tree.Query(new(70, 70, 20, 20));
      Assert.That(resBr, Has.Count.EqualTo(1));
      Assert.That(resBr[0].Id, Is.EqualTo(br.Id));

      // Top Left Query
      var resTl = _tree.Query(new(0, 0, 20, 20));
      Assert.That(resTl, Has.Count.EqualTo(1));
      Assert.That(resTl[0].Id, Is.EqualTo(tl.Id));
   }

   [Test]
   public void MidPoint_Calculations_ShouldBePrecise()
   {
      // Item exactly on the 50.0 boundary
      var boundaryItem = new TestItem(50.0f, 10, 10, 10);

      _tree.Insert(boundaryItem);
      _tree.Insert(new(10, 10, 2, 2));
      _tree.Insert(new(12, 12, 2, 2)); // Split trigger

      var root = GetRootNode();
      var rootItems = GetNodeItems(root);

      // Boundary item should stay in root
      Assert.That(rootItems, Does.Contain(boundaryItem));
      // Other items should be gone (moved to children)
      Assert.That(rootItems, Has.Count.EqualTo(1));
   }
}