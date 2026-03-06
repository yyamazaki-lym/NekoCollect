# 猫集めゲーム Unity セットアップ手順

## 1. プロジェクト作成

1. Unity Hub で新規プロジェクトを作成
   - テンプレート: **2D (URP)** または **2D**
   - プロジェクト名: `NekoCollect`
2. 作成後、`Assets/Scripts` フォルダにこのリポジトリの `Assets/Scripts` 以下を全てコピー

## 2. TextMeshPro のインストール

スクリプトが `TMPro` を使用しているため:
1. Window → Package Manager
2. TextMeshPro がインストールされていなければインストール
3. 初回利用時に「TMP Essentials」のインポートダイアログが出たらインポート

## 3. シーン構築

### 3.1 Manager オブジェクト

1. 空のGameObjectを作成、名前を `Managers` にする
2. 以下のコンポーネントをアタッチ:
   - `GameManager`
   - `SaveManager`
   - `CoinManager`
   - `GachaManager`
   - `CatCollectionManager`
   - `UIManager`

### 3.2 Canvas作成

1. UI → Canvas を作成
2. Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1080 x 1920`（縦画面）
   - Match: `0.5`

### 3.3 各画面パネル

Canvas の下に以下の Panel（空のGameObject + RectTransform）を作成:

#### HomePanel
```
HomePanel (GameObject)
├── CoinText (TextMeshPro)
├── ClickButton (Button) ← 大きめの猫画像ボタン
├── GachaButton (Button) ← 「ガチャ」
├── CatListButton (Button) ← 「猫一覧」
├── CatalogButton (Button) ← 「図鑑」
└── OfflineCoinPopup (GameObject) ← 初期非表示
    ├── OfflineCoinText (TextMeshPro)
    └── CloseButton (Button)
```
- `HomeUI` コンポーネントをアタッチして各UI参照を紐付け

#### GachaPanel
```
GachaPanel (GameObject)
├── BannerButtonContainer (HorizontalLayoutGroup)
├── SelectedBannerName (TextMeshPro)
├── CostText (TextMeshPro)
├── PullButton (Button) ← 「ガチャを引く」
├── BackButton (Button) ← 「戻る」
└── ResultPanel (GameObject) ← 初期非表示
    ├── ResultCatImage (Image)
    ├── ResultCatName (TextMeshPro)
    ├── ResultRarity (TextMeshPro)
    ├── ResultNewLabel (TextMeshPro)
    └── ResultCloseButton (Button)
```
- `GachaUI` コンポーネントをアタッチ
- `BannerButtonPrefab` 用のボタンPrefabを作成して参照にセット

#### CatListPanel
```
CatListPanel (GameObject)
├── ScrollView
│   └── Viewport
│       └── Content (GridLayoutGroup)
├── BackButton (Button)
```
- `CatListUI` コンポーネントをアタッチ
- `catGridContainer` に Content の Transform をセット
- `catCardPrefab` 用のPrefabを作成（Image + TextMeshPro×2 + Button）

#### CatDetailPanel
```
CatDetailPanel (GameObject)
├── CatImage (Image)
├── CatNameText (TextMeshPro)
├── RarityText (TextMeshPro)
├── LevelText (TextMeshPro)
├── ExpBar (Slider)
├── ExpText (TextMeshPro)
├── FriendlinessText (TextMeshPro)
├── EnergyText (TextMeshPro)
├── CutenessText (TextMeshPro)
├── SkillListContainer (VerticalLayoutGroup)
├── FeedCheapButton (Button)
│   └── FeedCheapCostText (TextMeshPro)
├── FeedPremiumButton (Button)
│   └── FeedPremiumCostText (TextMeshPro)
├── EvolveButton (Button)
│   └── EvolveInfoText (TextMeshPro)
└── BackButton (Button)
```
- `CatDetailUI` コンポーネントをアタッチ
- `skillItemPrefab` 用のPrefabを作成（TextMeshPro のみ）

#### CatalogPanel
```
CatalogPanel (GameObject)
├── CompletionText (TextMeshPro)
├── ScrollView
│   └── Viewport
│       └── Content (GridLayoutGroup)
├── BackButton (Button)
└── DetailPopup (GameObject) ← 初期非表示
    ├── DetailImage (Image)
    ├── DetailName (TextMeshPro)
    ├── DetailRarity (TextMeshPro)
    ├── DetailDescription (TextMeshPro)
    └── DetailCloseButton (Button)
```
- `CatalogUI` コンポーネントをアタッチ
- `catalogCardPrefab` 用のPrefabを作成

### 3.4 UIManager への参照設定

`UIManager` コンポーネントの各フィールドに、上で作成したパネルをドラッグ&ドロップ:
- `homePanel` → HomePanel
- `gachaPanel` → GachaPanel
- `catListPanel` → CatListPanel
- `catDetailPanel` → CatDetailPanel
- `catalogPanel` → CatalogPanel

## 4. マスターデータ作成

### 4.1 スキルデータ

1. Projectウィンドウで右クリック → Create → NekoCollect → SkillData
2. 以下のスキルを作成:

| スキル名 | 効果タイプ | 効果値 | 習得Lv |
|---|---|---|---|
| ゴロゴロ | IdleCoinBonus | 10 | 5 |
| 甘え上手 | ClickCoinBonus | 2 | 10 |
| お昼寝マスター | IdleCoinBonus | 20 | 15 |
| 猫パンチ | ClickCoinBonus | 5 | 8 |
| 学習能力 | ExpBonus | 15 | 3 |

### 4.2 猫データ

1. Create → NekoCollect → CatData
2. 例:

| catId | 名前 | レアリティ | なつき度 | 元気 | かわいさ | maxLevel |
|---|---|---|---|---|---|---|
| cat_mike | みけ | N | 5 | 3 | 4 | 20 |
| cat_kuro | くろ | N | 4 | 5 | 3 | 20 |
| cat_shiro | しろ | R | 8 | 6 | 7 | 25 |
| cat_tora | とら | R | 7 | 8 | 5 | 25 |
| cat_calico | 三毛姫 | SR | 12 | 10 | 15 | 30 |
| cat_persian | ペルシャ | SR | 15 | 8 | 20 | 30 |
| cat_king | ねこ大王 | SSR | 20 | 15 | 25 | 40 |

3. 各猫にスキルを割り当て、ドット絵スプライトを設定
4. 進化がある猫は `evolutionTarget` と `evolutionLevel` を設定

### 4.3 ガチャバナー

1. Create → NekoCollect → GachaBanner
2. 例:

**ノーマルガチャ（100コイン）:**
| 猫 | 重み |
|---|---|
| みけ | 30 |
| くろ | 30 |
| しろ | 20 |
| とら | 15 |
| 三毛姫 | 4 |
| ペルシャ | 0.9 |
| ねこ大王 | 0.1 |

### 4.4 CatCollectionManager への設定

1. `CatCollectionManager` コンポーネントの `allCats` 配列に全猫データをセット
2. `GachaManager` の `banners` 配列にガチャバナーをセット

## 5. ドット絵の準備

- 32x32 または 64x64 ピクセルのドット絵を作成
- Import Settings:
  - Texture Type: `Sprite (2D and UI)`
  - Pixels Per Unit: `32` (または `64`)
  - Filter Mode: `Point (no filter)` ← ドット絵に必須
  - Compression: `None`

## 6. WebGLビルド設定

1. File → Build Settings
2. Platform: WebGL を選択して Switch Platform
3. Player Settings:
   - Resolution: `960 x 540`（または好みのサイズ）
   - WebGL Template: Default
4. Build And Run でテスト

## 7. 動作確認チェックリスト

- [ ] ホーム画面でクリックするとコインが増える
- [ ] コインが十分な状態でガチャを引ける
- [ ] 新規猫が図鑑に登録される
- [ ] 猫一覧から猫を選択して詳細画面が開く
- [ ] エサを与えてレベルアップする
- [ ] スキルが自動習得される
- [ ] 進化条件を満たすと進化できる
- [ ] 図鑑で未取得猫がシルエット表示される
- [ ] ブラウザを閉じて再度開くとセーブデータが復元される
- [ ] 放置後にログインすると「おるすばんボーナス」が表示される
