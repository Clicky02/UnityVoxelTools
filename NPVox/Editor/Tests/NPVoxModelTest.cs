using UnityEngine;
using System.Collections;
using System;
using NUnit.Framework;

public class NPVoxModelTest
{
    [Test]
    public void Clamp_ShouldReturnClampedVoxCoord()
    {
        VoxModel sut = VoxModel.NewInstance(new VoxCoord(3, 3, 3));
        Assert.AreEqual(new VoxCoord(1, 1, 1), sut.Clamp(new VoxCoord(1, 1, 1)));
        Assert.AreEqual(new VoxCoord(0, 0, 0), sut.Clamp(new VoxCoord(-2, -2, -2)));
        Assert.AreEqual(new VoxCoord(2, 2, 2), sut.Clamp(new VoxCoord(7, 7, 7)));
    }

    [Test]
    public void Clamp_ShouldReturnClampedVoxBox()
    {
        VoxModel sut = VoxModel.NewInstance(new VoxCoord(3, 3, 3));
        NPVoxBox box = sut.Clamp(new NPVoxBox(new VoxCoord(-2, -2, -2), new VoxCoord(6, 6, 6)));

        Assert.AreEqual(new VoxCoord(2, 2, 2), box.RightUpForward);
        Assert.AreEqual(new VoxCoord(0, 0, 0), box.LeftDownBack);
    }

}
