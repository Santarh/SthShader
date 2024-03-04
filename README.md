# SthShader

## How to Install
Unity 上部メニューバーから `Window -> Package Manager` を選択し、 `+` ボタンを押して `Add package from git URL...` を選択します。

そして入力欄に以下の URL を入力し、 `Add` ボタンを押します。

```
https://github.com/Santarh/SthShader.git?path=/Packages/net.santarh.sth-shader
```

## SDF Generator
白黒二値テクスチャを入力に、SDF（符号付き距離場）テクスチャを生成します。

`Window -> Sth Shader -> SDF Generator` を選択します。

`Input Texture` に入力テクスチャをアサインします。
この入力テクスチャは内容として白黒画像である必要があり、白が「領域内」を示します。
またテクスチャは `Read/Write` フラグが ON である必要があり、非圧縮であることが望ましいです。
これは `Fix Texture Import Settings` ボタンを押すことで修正できます。

最後に `Generate` ボタンを押します。
ファイル保存ダイアログで保存先を選び、画像ファイルを出力します。

## Shadow Threshold Map Generator
連番白黒二値テクスチャを入力に、Shadow Threshold Map（陰影しきい値マップ）を生成します。

`Window -> Sth Shader -> Shadow Threshold Map Generator` を選択します。

`Input Textures` に入力連番テクスチャをアサインします。
白領域の少ない順にアサインする必要があります。


最後に `Generate` ボタンを押します。
ファイル保存ダイアログで保存先を選び、画像ファイルを出力します。
