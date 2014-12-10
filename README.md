ConfigParser
============

PythonのConfigParserをC#向けにコピーしたものです。
細かい動作は若干異なっています。

INIファイルを読み込む方法としては[DllImport("kernel32")]を使う方法もあるようですが
このコードではdllは使わず、ConfigParserを利用しています。
そのため、該当のdllが無い環境でも動く…はず。多分。
