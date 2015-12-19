using PCG3.TestFramework;
using System.Threading;

namespace PCG3.TestUnitTests {

  /// <summary>
  /// Set of long running tests.
  ///  - 5 tests have a duration of  5 seconds each.
  ///  - 5 tests have a duration of 10 seconds each.
  ///  - 5 tests have a duration of 15 seconds each.
  /// </summary>
  public class LongRunningTests {

    #region 5 seconds
    [Test]
    public void Sec5Test1() {
      Thread.Sleep(5000);
    }

    [Test]
    public void Sec5Test2() {
      Thread.Sleep(5000);
    }

    [Test]
    public void Sec5Test3() {
      Thread.Sleep(5000);
    }

    [Test]
    public void Sec5Test4() {
      Thread.Sleep(5000);
    }

    [Test]
    public void Sec5Test5() {
      Thread.Sleep(5000);
    }
    #endregion


    #region 10 seconds
    [Test]
    public void Sec10Test1() {
      Thread.Sleep(10000);
    }

    [Test]
    public void Sec10Test2() {
      Thread.Sleep(10000);
    }

    [Test]
    public void Sec10Test3() {
      Thread.Sleep(10000);
    }

    [Test]
    public void Sec10Test4() {
      Thread.Sleep(10000);
    }

    [Test]
    public void Sec10Test5() {
      Thread.Sleep(10000);
    }
    #endregion


    #region 15 seconds
    [Test]
    public void Sec15Test1() {
      Thread.Sleep(15000);
    }

    [Test]
    public void Sec15Test2() {
      Thread.Sleep(15000);
    }

    [Test]
    public void Sec15Test3() {
      Thread.Sleep(15000);
    }

    [Test]
    public void Sec15Test4() {
      Thread.Sleep(15000);
    }

    [Test]
    public void Sec15Test5() {
      Thread.Sleep(15000);
    }
    #endregion
  }
}
