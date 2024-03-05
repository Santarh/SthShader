# SthShader

## インストール方法
動作環境は Unity 2022.3 です。

Unity 上部メニューバーから `Window -> Package Manager` を選択し、 `+` ボタンを押して `Add package from git URL...` を選択します。

そして入力欄に以下の URL を入力し、 `Add` ボタンを押します。

```
https://github.com/Santarh/SthShader.git?path=/Packages/net.santarh.sth-shader
```

# Tools

## SDF Generator
白黒二値テクスチャを入力に、SDF（符号付き距離場）テクスチャを生成します。

上部メニューバーから `Sth Shader -> SDF Generator` を選択し、ウインドウを表示します。

### 使い方
1. `Input Texture` に入力テクスチャをアサインします。
2. `Fix Texture Import Settings` ボタンを押し、入力テクスチャの設定を正しくします。
3. `Generate` ボタンを押し、出力先を決めます。

> 生成したテクスチャは `Sth Shader/UnlitSdf` シェーダでプレビューすることができます。

<img width="400" src="https://github.com/Santarh/SthShader/assets/328204/38960827-7261-4a53-9754-367d336fd4f1">

### 詳しい使い方

#### `Input Texture`
入力テクスチャは白黒の二値マスク画像が求められます。
白が「領域内」を示します。

#### `Fix Texture Import Settings`
入力テクスチャが満たすべき次の設定を自動で修正します。

- `Read/Write` フラグが ON である
- `Compression` が `None` である

#### `Inner Mask Threshold`
`Advanced Settings` メニュー内にある、入力テクスチャのマスクのしきい値を決めることができます。

- しきい値よりも大きい値が「領域内」であると判定されます。
- しきい値は RGB チャンネルそれぞれに設定できます。
- RGB チャンネルのうちどれか一つでも「領域内」だと判定されれば、そのピクセルは「領域内」とみなされます。

> たとえば R チャンネルのしきい値を `0.0` にすると「R の色が 0 (黒) より明るければ「領域内」であると見做します。
> 逆にしきい値を `1.0` にすると、それより明るい色は通常ないので、すべてが「領域外」になります。


## Shadow Threshold Map Generator
連番白黒二値テクスチャを入力に、Shadow Threshold Map（陰影しきい値マップ）を生成します。

上部メニューバーから `Sth Shader -> Shadow Threshold Map Generator` を選択します。

`Input Textures` に入力連番テクスチャをアサインします。
白領域の少ない順にアサインする必要があります。


最後に `Generate` ボタンを押します。
ファイル保存ダイアログで保存先を選び、画像ファイルを出力します。

テクスチャは `Sth Shader/UnlitSdf` シェーダでプレビューすることができます。
