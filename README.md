# ClothSimulationInRust
This is my Simulation Stuff Gi

## 首先展示一下效果图

![Cloth Simulation Effect](/image/cloth_effect.gif)

* 布料模拟的本质是弹簧模拟，弹簧模拟有一个**胡克定律**；根据胡克定律弹簧发生形变时会产生一个与其**弹性系数**和**形变量**相关的力；
* 根据布料的结构这种弹性力可以分为横，纵，斜三个方向的约束力。这三个力统称为**内部力**
* 除了内部力，布料还会受到诸如重力，风力，碰撞等**外部力**；

我这里采用了质点来模拟布料的节点，每次迭代过程通过依次计算各个质点之间产生的**内部力**，**外部力**和与物体碰撞产生的垂直于碰撞切面的力。通过迭代这个力叠加产生位移的过程来计算出布料各个质点更新的位置，从而模拟出布料的效果；

这里质点受力采用的是最简单的**欧拉法**，欧拉法是我们接触的比较多的也比较好理解的运动学微分法，通是力 - 加速度 - 速度 - 位移这个微分的过程；这个方法也是效果比较一般的解法；目前google过来的话**Verlet积分法**是比较高效和稳定的解算法，可以后期了解一下这一块。

## 这里展示一下Rust的学习

![rustlearn](/image/rustlearn.png)

rust这里我自己找了一些学习资料，因为是从零开始。为了避免从入门到放弃，这边找了一些很基础的学习资料诸如菜鸟教程，rust圣经（前半部分）等。

另外也使用了一些你这边给的资料rustlings，另一个基于项目的教程还没来的及看到；总的来说rustlings可以把问题讲的比较清楚，不懂得圣经那边也可以查到然后就是google了。总的来说我应该还只接触了rust一些比较初期的部分，但是足以用它们完成这部分的test了，我也就先着手做了。

![rustlib](/image/rustlib.png)

以上是做的布料解算的rust实现；这个部分分成四步；
* 用cargo创建一个--lib的项目文件
* 实现一个Vector3的结构体，以及实现一些其需要的基本函数；实现这个部分主要是因为在解算的部分若vector可以作为基础类型直接传递，那么会省下很多时间去重构代码；
* 解算的实现，这里需要注意给函数加mangle这个宏，来保证函数名在编译时不会被改变，有助于其它语言的调用（C#这边要调用需要做的事情）
* 单元测试，对一些基本函数可以用单元测试测试其准确性，对于一些应用于Unity里面的function例如is_inside_sphere()我们可以替换或者到Unity中去测试其准确性

## 性能对比

在rust的学习过程中，了解到rust是一个特别严谨且高效的语言，于是在搬运解算算法的时候就萌生了对比性能消耗的想法；以下是将解算中，性能消耗的热点部分搬移到rust后的消耗和在C#中的消耗做的对比；

![C#](/image/C%23_cost.png)
C#的性能消耗↑

![rust](/image/rust_cost.png)
rust的性能消耗↑

可以看到，在这样一个简单的解算下，性能提升就已经达到如此大的程度了，可见rust的性能还是很好的！

## 图形渲染

对于图形渲染这一块我也花了一些时间学习，这一块我用的是unity中mesh来渲染的；当我们需要自定义mesh时，要了解mesh这个类的构成；一个mesh类的渲染我们需要预设好它的节点vertices，贴图uv，以及渲染单元(我们这里用的是triangles)等等（以上是最基本的参数，还有很多其他的）。在设置渲染时我们需要编排好质点的数据存储的顺序结构，以便于我们在渲染mesh时tiangles和vertices的顺序时，能较方便的找到他们的序号。在运行过程中我们对于已经构建好的mesh只需要改变其vertices的位置就可以做到对整个mesh形态的影响了。