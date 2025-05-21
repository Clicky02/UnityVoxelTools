using UnityEngine;
using System.Collections;
using System;
using NUnit.Framework;

public class NPVoxBoxTest
{
    [Test]
    public void Size_ShouldReturnCorrectSize()
    {
        NPVoxBox sut = new NPVoxBox(new VoxCoord(1, 1, 1), new VoxCoord(4, 4, 4));
        Assert.AreEqual(4, sut.Size.x);
        Assert.AreEqual(4, sut.Size.y);
        Assert.AreEqual(4, sut.Size.z);
    }

    [Test]
    public void Center_ShouldReturnCorrectCenter()
    {
        NPVoxBox sut = new NPVoxBox(new VoxCoord(1, 1, 1), new VoxCoord(3, 3, 3));
        Assert.AreEqual(2, sut.Center.x);
        Assert.AreEqual(2, sut.Center.y);
        Assert.AreEqual(2, sut.Center.z);
    }

    [Test]
    public void FromCenterSize_ShouldConstructCorrectBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new VoxCoord(2, 2, 2), new VoxCoord(3, 3, 3));
        Assert.AreEqual(2, sut.Center.x);
        Assert.AreEqual(2, sut.Center.y);
        Assert.AreEqual(2, sut.Center.z);
        Assert.AreEqual(3, sut.Size.x);
        Assert.AreEqual(3, sut.Size.y);
        Assert.AreEqual(3, sut.Size.z);
        Assert.AreEqual(1, sut.LeftDownBack.x);
        Assert.AreEqual(1, sut.LeftDownBack.y);
        Assert.AreEqual(1, sut.LeftDownBack.z);
        Assert.AreEqual(3, sut.RightUpForward.x);
        Assert.AreEqual(3, sut.RightUpForward.y);
        Assert.AreEqual(3, sut.RightUpForward.z);
    }

    [Test]
    public void Contains_ShouldReturnTrueWhenInBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new VoxCoord(2, 2, 2), new VoxCoord(3, 3, 3));
        Assert.IsTrue(sut.Contains(new VoxCoord(2, 2, 2)));
        Assert.IsTrue(sut.Contains(new VoxCoord(1, 1, 1)));
        Assert.IsTrue(sut.Contains(new VoxCoord(3, 3, 3)));
    }

    [Test]
    public void Contains_ShouldReturnFalseWhenInBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new VoxCoord(2, 2, 2), new VoxCoord(3, 3, 3));
        Assert.IsFalse(sut.Contains(new VoxCoord(0, 0, 0)));
        Assert.IsFalse(sut.Contains(new VoxCoord(4, 4, 4)));
    }
}
