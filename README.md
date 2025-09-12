# YjjTool 使用文档

## 概述
YjjTool是一个功能丰富的Unity工具包，提供了多种UI组件、图表系统、相机控制、3D转2D UI等功能。所有组件都支持在编辑器中直接调整参数并实时预览效果，无需编写代码即可实现复杂的交互效果。

##依赖
TextMeshRro(可通过PackageManager导入)
NewtonsoftJson(可通过PackageManager导入)
Odin
---

## 1. 3D转2D UI组件 (3DTo2DPointUI)

### PointUI 组件
将3D世界中的物体位置映射到2D UI界面，支持动态跟随、缩放和动画效果。

#### 主要参数：

**基础设置**
- `scaleWithDistance` (bool): 根据距离缩放UI元素
- `perfectDistance` (float): 完美距离，用于距离缩放计算
- `point` (Transform): 3D场景中要跟踪的目标点
- `offsets` (List<Vector2>): 偏移量列表，用于调整UI位置

**绘制设置**
- `drawLine` (bool): 是否绘制连接线
- `width` (float): 线条宽度
- `lineColor` (Color): 线条颜色
- `aligin` (Aligin): 对齐方式
  - 居左对齐
  - 居右对齐  
  - 居中对齐

**内容控制**
- `controllImage` (RectTransform): 要控制显示的UI内容
- `imageOffsetIndex` (int): 内容基于第几个数据点偏移
- `imageOffset` (Vector2): 内容偏移值
- `autoLength` (bool): 自动计算横轴长度
- `pointImage` (Image): 指向点的图片

**动画设置**
- `openAnimation` (bool): 开启动画
- `animationTime` (float): 动画时间

### PointUILayerUpdater 组件
自动管理多个PointUI的层级顺序，根据与相机的距离动态调整显示顺序。

#### 主要参数：
- 自动获取子物体中的所有PointUI组件
- 根据相机位置自动排序
- 支持相机移动事件监听

---

## 2. 按钮功能组件 (ButtonFunction)

### SingleButtonFunction 组件
增强的按钮功能，支持状态切换、悬停效果、二次确认等功能。

#### 主要参数：

**基础功能**
- `clickSprite` (Sprite): 点击时显示的图片
- `needDoubleSure` (bool): 是否需要二次确认
- `isClick` (bool, 只读): 当前是否按下状态

**悬停效果**
- `openHover` (bool): 开启悬停效果
- `hoverTextColor` (Color): 悬停时文字颜色
- `hoverSprite` (Sprite): 悬停时显示的图片

**视觉变化**
- `setNativesize` (bool): 改变sprite后设置为原大小
- `changeTextColor` (bool): 改变文字颜色
- `changeColor` (Color): 改变后的文字颜色
- `changeSpriteColor` (bool): 改变sprite颜色
- `spriteColor` (Color): 改变后的sprite颜色

**初始化设置**
- `firstIsClick` (bool): 初始是否为按下状态
- `invokeEventAtStart` (bool): 初始化是否执行事件
- `disable2Reset` (bool): 消失时切换初始状态
- `enable2SetState` (bool): 重新显示时切回初始状态
- `disable2UnClick` (bool): 隐藏时切换成未点击

**显示控制**
- `shows` (List<GameObject>): 打开时显示的对象列表
- `hideShowsOnClose` (bool): 关闭时是否隐藏显示对象
- `hides` (List<GameObject>): 打开时隐藏的对象列表
- `showHidesOnClose` (bool): 关闭时是否显示隐藏对象

**事件**
- `ClickEvent` (UnityEvent): 点击事件
- `CancelEvent` (UnityEvent): 取消事件
- `StateEvent` (BoolEvent): 状态变化事件
- `OnHoverChange` (UnityEvent<bool>): 悬停状态变化事件

### DoubleSure 组件
二次确认对话框系统。

#### 主要参数：
- `window` (GameObject): 确认窗口对象
- `sureButton` (Button): 确认按钮
- `cancelButton` (Button): 取消按钮
- `msg` (TextMeshProUGUI): 提示信息文本

### FocusAnimation 组件
鼠标悬停时的缩放动画效果。

#### 主要参数：
- `target` (Transform): 动画目标（为空则为自己）
- `animationTime` (float): 动画时间（秒）
- `curve` (AnimationCurve): 动画曲线
- `scaler` (Vector3): 缩放系数

### ButtonGroup 组件
按钮组管理，支持单选模式。

#### 主要参数：
- `supportCancel` (bool): 点击同一个按钮是否支持取消
- `clearOnEnabel` (bool): enable时取消已点击的按钮
- `clearOnDisabel` (bool): 按钮被隐藏时取消已点击的按钮
- `HaveButtonClickEvent` (UnityEvent): 有按钮点击时的事件
- `ClearEvent` (UnityEvent): 清除事件

---

## 3. 相机控制器 (Camera)

### CameraController 组件
功能强大的相机控制系统，支持多种输入方式和相机切换。

#### 主要参数：

**射线检测设置**
- `rayType` (int): 射线类型
  - 0: 相交公式计算
  - 1: 射线检测
- `ground` (Transform): 地面对象
- `clampBox` (BoxCollider): 相机移动限制区域

**缩放控制**
- `openScale` (bool): 是否根据高度开启阻塞
- `minMoveSpeed` (float): 最小移动速度

**相机设置**
- `set` (CameraSet): 相机控制属性
- `beginCamera` (CameraInfo): 初始相机
- `inputMoveSpeed` (float): 输入移动速度

**状态信息**
- `canMove` (bool, 只读): 是否可以移动
- `currentFocus` (Transform, 只读): 当前焦点

### CameraSet 配置
相机控制的具体参数设置。

#### 主要参数：
- `rotateSpeed` (float): 镜头旋转速度
- `moveSpeed` (float): 镜头远近速度
- `moveSmoothTime` (float): 移动平滑时间
- `rotateDamping` (float): 旋转阻尼
- `forwardDamping` (float): 前进阻尼
- `minHeigh` (float): 与地面最低高度
- `minAngle` (float): 镜头最低角度
- `minDistance` (float): 镜头与地面最近距离
- `focusTime` (float): 聚焦时间
- `focusDistance` (float): 聚焦距离
- `nearDistanceThreshold` (float): 镜头远近距离阈值

---

## 4. 图表系统 (Charts)

### 图表基础类 (ChartBase)
所有图表的基类，提供通用的图表功能。

#### 主要参数：
- `setWithoutSetData` (bool): 没有读取数据时awake是否播放动画

### 自由图表 (FreeChart)
最灵活的图表组件，支持多种图表类型的组合。

#### 主要参数：

**基础设置**
- `set` (BaseSet): 基础设置
- `hoverSet` (HoverSet): 悬停设置
- `HoverEvent` (IntEvent): 悬停事件
- `HoverExitEvent` (IntEvent): 悬停退出事件
- `GetHoverNameEvent` (stringEvent): 获取悬停名称事件

**数据设置**
- `dataSet` (DataSet): 数据标题设置
- `datas` (List<MultipleData>): 数据列表
- `charts` (List<DrawFreeChartBase>): 图表列表
- `showUnit` (bool): 是否显示数据单位
- `font` (TMP_FontAsset): 字体

**动画设置**
- `animationSet` (AnimationSet): 动画设置

### 3D柱状图 (Yjj_3dBarGraph)
3D柱状图组件，支持多组数据对比。

#### 主要参数：

**基础设置**
- `set` (BaseSet): 基础设置
- `hoverSet` (HoverSet): 悬停设置
- `barSet` (Yjj_3dBarDrawer.Bar3DSet): 3D柱状图设置

**数据设置**
- `dataSet` (DataSet): 数据标题设置
- `datas` (List<MultipleData>): 数据列表
- `barWidth` (float): 柱状图宽度
- `distance` (float): 柱状图间距
- `colorList` (List<Color>): 柱状图颜色列表

**文本显示**
- `openDataText` (bool): 开启数据文本
- `showUnit` (bool): 显示单位
- `textSize` (float): 文本大小
- `textOffset` (Vector2): 文本偏移
- `textEnd` (int): 保留几位小数
- `textFont` (TMP_FontAsset): 文本字体
- `textColor` (Color): 文本颜色

**动画设置**
- `animationSet` (AnimationSet): 动画设置
- `openLoop` (bool): 是否开启循环动画

### 饼图 (Yjj_PieChartNew)
功能丰富的饼图组件，支持多种显示模式和动画效果。

#### 主要参数：

**基础数据**
- `datas` (List<float>): 数据列表
- `names` (List<string>): 名称列表
- `colors` (List<Color>): 颜色列表

**形状设置**
- `distanceAngle` (float): 间隔角度
- `width` (float): 宽度
- `smooth` (int): 细分程度
- `startAngle` (float): 起始角度
- `roundRadiu` (float): 圆角半径
- `radius` (float, 只读): 半径

**背景设置**
- `drawBackGround` (bool): 绘制底板
- `backGroundSmooth` (int): 背景细分

**交互设置**
- `openHover` (bool): 开启悬停
- `uicamera` (Camera): UI相机

**画线设置**
- `drawLine` (bool): 启用画线
- `lineWidth` (float): 线条宽度
- `lineColor` (Color): 线条颜色
- `lineOffset` (Vector2): 线条偏移
- `lineLength` (float): 线条长度
- `textInCenter` (bool): 文本居中

**文本设置**
- `textType` (TitleType): 选择显示内容
  - 不显示
  - 显示数据
  - 显示标题
  - 显示标题和数据
- `titleSize` (float): 标题大小
- `titleColor` (Color): 标题颜色
- `valueTextColorFollowSprite` (bool): 开启数据文本颜色跟随
- `floatCount` (int): 数据小数位数
- `textDistance` (float): 文本距离中心的距离
- `text_color` (Color): 文本颜色
- `dataColor` (Color): 数据颜色
- `text_size` (float): 文本大小
- `font` (TMP_FontAsset): 字体
- `showUnit` (bool): 是否显示单位
- `unit` (string): 单位

**图例设置**
- `enableLegend` (bool): 启用图例
- `legendWithData` (bool): 图例里是否显示数据
- `config` (Yjj_LegendConfig): 图例配置

**动画设置**
- `enableAnimation` (bool): 启用动画
- `animationTime` (float): 动画时间
- `animationType` (AnimationType): 动画类型
  - Sequential: 顺序播放
  - CenterOut: 从中心向外
  - OutsideIn: 从外向内
- `animationCurve` (AnimationCurve): 动画曲线
- `staggerDelay` (float): 错开延迟
- `openLoop` (bool): 开启循环
- `loopScale` (float): 循环缩放
- `loopSpaceTime` (float): 循环间隔时间
- `loopCurve` (AnimationCurve): 循环曲线
- `LoopEvent` (UnityEvent<int>): 循环事件
- `fadeInTime` (float): 渐入时间
- `fadeOutTime` (float): 渐出时间

### 水波图 (WaterChart)
水波效果的数据展示组件。

#### 主要参数：

**数据显示**
- `data` (float): 当前数据值
- `maxValue` (float): 最大值
- `type` (ShowType): 显示类型
  - 显示百分比
  - 显示原始数据
- `floatCount` (int): 文本保留小数位数

**动画设置**
- `fadeInTime` (float): 渐入动画时间

**材质表现**
- `speed` (float): 波浪运动速度
- `am` (float): 振幅
- `waterColor` (Color): 颜色
- `lineColor` (Color): 边线颜色
- `lineWidth` (float): 边线宽度

### 热力图 (Yjj_HeatMap)
基于3D位置数据的热力图生成组件。

#### 主要参数：

**数据设置**
- `pointsList` (List<Vector3>): 3D位置点列表
- `dataList` (List<float>): 对应的数据值列表
- `plane` (GameObject): 显示热力图的平面对象

**渲染设置**
- `damping` (float): 衰减系数
- `dampingLevel` (int): 衰减随机系数
- `minLength` (int): 最小辐射范围
- `maxPercent` (float): 最大值映射
- `curveRamap` (bool): 取颜色最大值进行映射
- `ct` (ComputeType): 计算类型
  - 像素直接相加
  - 像素加权相加
- `curve` (AnimationCurve): 颜色曲线

**Excel读取**
- `excelPath` (string): 数据表格位置
- `readDataAtAwake` (bool): awake时是否读取excel
- `dataIndex` (int): 数据所在excel位置

**渲染参数**
- `maxPix` (int): 图片最长的一边的像素

### 悬停设置 (HoverSet)
图表悬停效果的配置类。

#### 主要参数：

**基础设置**
- `active` (bool): 开启hover功能
- `offset` (Vector2): 弹窗基于鼠标偏移

**悬停效果**
- `hoverScale` (float): hover时图表缩放系数
- `hoverColor` (Color): hover改变颜色
- `hoverRect` (RectTransform): hover时对应位置显示的垂直线

**UI设置**
- `uicamera` (Camera): UI相机
- `root` (Transform): 弹窗根节点
- `valueTextList` (List<TextMeshProUGUI>): 用于接收并显示数值的文本
- `nameText` (TextMeshProUGUI): 用于显示标题的文本

---

## 5. 工具类组件

### YjjUtility 工具类
提供各种实用的工具方法。

#### 主要功能：
- **动画系统**: FadeIn、FadeOut等动画方法
- **延迟执行**: Delay、DelayWhile等延迟方法
- **随机功能**: Probability概率判断
- **调试工具**: InspectObject对象检查
- **性能分析**: BeginSample、EndSample性能计时
- **深度复制**: DeepCopyUsingBinarySerialization深度复制

### MultipleData 数据类
用于图表的多维数据结构。

#### 主要参数：
- `datas` (List<float>): 数据列表

#### 主要方法：
- `GetDatas()`: 静态方法，用于创建MultipleData列表

---

## 6. 配置系统

### V2BaseSet 配置类
图表V2版本的基础配置。

#### 主要参数：

**字体设置**
- `font` (TMP_FontAsset): 字体资源

**距离设置**
- `distanceFromTop` (float): 与顶部距离
- `distanceFromButtom` (float): 与底部距离

**最大最小值设置**
- `autoMax` (bool): 自动最大值
- `max` (float): 手动设置最大值
- `autoMin` (bool): 自动最小值
- `min` (float): 手动设置最小值

**布局设置**
- `dataMinDistance` (float): 数据最小间隔像素
- `useCenterPosition` (bool): 启用中心位置模式
- `distanceFromLeft` (float): 与左边距离
- `distanceFromRight` (float): 与右边距离
- `colors` (List<Color>): 颜色列表

**图例设置**
- `seriesNames` (List<string>): 数据系列名称

**动画参数**
- `openAnimation` (bool): 开启动画
- `fadeInTime` (float): 渐入时间
- `curve` (AnimationCurve): 动画曲线

---

## 使用说明

### 基本使用流程
1. 将相应的组件添加到GameObject上
2. 在Inspector面板中调整参数
3. 参数调整后会自动刷新效果
4. 在运行时可以通过代码调用相关方法

### 注意事项
- 大部分组件都支持在编辑器中实时预览
- 图表组件需要正确设置数据才能正常显示
- 相机控制器需要配合Cinemachine使用
- 3D转2D UI需要正确设置相机引用
- 悬停效果需要正确设置UI相机

### 扩展功能
- 所有组件都支持通过继承进行功能扩展
- 事件系统支持自定义回调
- 动画系统支持自定义曲线
- 图表系统支持自定义绘制组件

---

## 版本信息
- 当前版本: 基于Unity 2022.3 LTS
- 兼容性: 支持PC、移动端、WebGL平台
