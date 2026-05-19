// Editor-only build tool – run via: Tools → AgeDefense → Build HUD
// Delete this file after the HUD has been generated.
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AgeDefenseUIBuilder
{
    [MenuItem("Tools/AgeDefense/Build HUD")]
    public static void BuildHUD()
    {
        // ── Find scene objects ────────────────────────────────────────────────
        var canvasGO = GameObject.Find("HUD_Canvas");
        if (canvasGO == null) { Debug.LogError("[UIBuilder] HUD_Canvas not found!"); return; }

        var hud = canvasGO.GetComponent<GameHUD>();
        if (hud == null) { Debug.LogError("[UIBuilder] GameHUD not on HUD_Canvas!"); return; }

        var so = new SerializedObject(hud);

        // ── Sprites from Tiny Sword ───────────────────────────────────────────
        Sprite sBigBarBase    = SP("Bars/BigBar_Base.png");
        Sprite sBigBarFill    = SP("Bars/BigBar_Fill.png");
        Sprite sBigBlueBtn    = SP("Buttons/BigBlueButton_Regular.png");
        Sprite sBigRedBtn     = SP("Buttons/BigRedButton_Regular.png");
        Sprite sSmallBlueSq   = SP("Buttons/SmallBlueSquareButton_Regular.png");
        Sprite sTinyRedRound  = SP("Buttons/TinyRoundRedButton.png");
        Sprite sTinyBlueRound = SP("Buttons/TinyRoundBlueButton.png");
        Sprite sBanner        = SP("Banners/Banner.png");
        Sprite sWoodTable     = SP("Wood Table/WoodTable.png");
        Sprite sRegularPaper  = SP("Papers/RegularPaper.png");
        Sprite sSpecialPaper  = SP("Papers/SpecialPaper.png");
        Sprite sBigRibbon1    = SP("Ribbons/BigRibbons 1.png");
        // Building sprites for the build-panel buttons
        Sprite[] buildingSprites = new Sprite[5];
        buildingSprites[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/Buildings/Blue Buildings/Barracks.png");
        buildingSprites[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/Buildings/Blue Buildings/Archery.png");
        buildingSprites[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/Buildings/Blue Buildings/Tower.png");
        buildingSprites[3] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/Buildings/Blue Buildings/Monastery.png");
        buildingSprites[4] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny Swords/Buildings/Blue Buildings/House1.png");

        // ── Helper to remove old build if re-run ─────────────────────────────
        void DestroyChild(string name)
        {
            var t = canvasGO.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
        DestroyChild("CastleHPBar");
        DestroyChild("TimerPanel");
        DestroyChild("WaveBanner");
        DestroyChild("BuildPanel");
        DestroyChild("BottomActionBar");
        DestroyChild("ActionBtn_Boost");
        DestroyChild("ActionBtn_Reparar");
        DestroyChild("ActionBtn_Peon");
        DestroyChild("ActionBtn_Mejorar");
        DestroyChild("GameOverPanel");
        DestroyChild("PausePanel");
        DestroyChild("BuildingInfoPanel");
        DestroyChild("PawnInfoPanel");

        // ════════════════════════════════════════════════════════════════════
        // 1. CASTLE HP BAR – top-left
        // ════════════════════════════════════════════════════════════════════
        var hpRoot = MakePanel(canvasGO, "CastleHPBar",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(110, -20), new Vector2(220, 30));
        // base layer (background bar)
        var hpBgGO = MakePanel(hpRoot, "Base",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetStretch(hpBgGO);
        var hpBgImg = hpBgGO.AddComponent<Image>();
        if (sBigBarBase != null) { hpBgImg.sprite = sBigBarBase; hpBgImg.type = Image.Type.Simple; hpBgImg.preserveAspect = false; }
        else hpBgImg.color = new Color(0.2f, 0.1f, 0.1f, 1f);
        // fill layer (green health)
        var hpFillGO = MakePanel(hpRoot, "Fill",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetStretch(hpFillGO);
        var hpFillImg = hpFillGO.AddComponent<Image>();
        if (sBigBarFill != null) { hpFillImg.sprite = sBigBarFill; hpFillImg.type = Image.Type.Simple; hpFillImg.preserveAspect = false; }
        else hpFillImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpFillImg.fillAmount = 1f;
        // label
        var hpLbl = MakeTMP(hpRoot, "HPLabel", "Castillo", 13, new Vector2(0, 0), new Vector2(200, 28));
        hpLbl.alignment = TextAlignmentOptions.Center;
        hpLbl.color = Color.white;
        hpLbl.fontStyle = FontStyles.Bold;

        // ════════════════════════════════════════════════════════════════════
        // 2. GAME TIMER – top-right
        // ════════════════════════════════════════════════════════════════════
        var timerPanel = MakePanel(canvasGO, "TimerPanel",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-70, -20), new Vector2(130, 30));
        var timerBg = timerPanel.AddComponent<Image>();
        timerBg.color = new Color(0, 0, 0, 0.55f);
        var timerTMP = MakeTMP(timerPanel, "TimerText", "00:00", 22, Vector2.zero, new Vector2(125, 28));
        timerTMP.alignment = TextAlignmentOptions.Center;
        timerTMP.color = Color.white;
        timerTMP.fontStyle = FontStyles.Bold;

        // ════════════════════════════════════════════════════════════════════
        // 3. WAVE BANNER – center-upper, starts hidden
        // ════════════════════════════════════════════════════════════════════
        var waveBannerRoot = MakePanel(canvasGO, "WaveBanner",
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
            Vector2.zero, new Vector2(420, 90));
        var wbBgImg = waveBannerRoot.AddComponent<Image>();
        if (sBanner != null) { wbBgImg.sprite = sBanner; wbBgImg.type = Image.Type.Simple; wbBgImg.preserveAspect = false; }
        else wbBgImg.color = new Color(0.1f, 0.05f, 0.02f, 0.9f);
        var waveBannerTMP = MakeTMP(waveBannerRoot, "WaveBannerText", "¡Oleada!", 26, new Vector2(0, 14), new Vector2(400, 40));
        waveBannerTMP.alignment = TextAlignmentOptions.Center;
        waveBannerTMP.color = new Color(1f, 0.9f, 0.2f, 1f);
        waveBannerTMP.fontStyle = FontStyles.Bold;
        var waveCountTMP = MakeTMP(waveBannerRoot, "WaveCountdownText", "30", 20, new Vector2(0, -18), new Vector2(400, 30));
        waveCountTMP.alignment = TextAlignmentOptions.Center;
        waveCountTMP.color = Color.white;
        waveBannerRoot.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // 4. BUILD PANEL – bottom-center, starts hidden
        // ════════════════════════════════════════════════════════════════════
        var buildPanel = MakePanel(canvasGO, "BuildPanel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 90), new Vector2(500, 160));
        var bpBgImg = buildPanel.AddComponent<Image>();
        if (sWoodTable != null) { bpBgImg.sprite = sWoodTable; bpBgImg.type = Image.Type.Simple; bpBgImg.preserveAspect = false; }
        else bpBgImg.color = new Color(0.35f, 0.22f, 0.1f, 0.97f);
        // Title
        var bpTitle = MakeTMP(buildPanel, "BuildTitle", "Construir", 18, new Vector2(0, 62), new Vector2(480, 26));
        bpTitle.alignment = TextAlignmentOptions.Center;
        bpTitle.color = new Color(1f, 0.9f, 0.5f, 1f);
        bpTitle.fontStyle = FontStyles.Bold;
        // 5 building buttons: Barracks, Archery, Tower, Monastery, House
        string[] bNames  = { "Cuartel",   "Arqueria", "Torre",    "Monasterio", "Casa"  };
        string[] bCosts  = { "80 Mad",    "60 Mad",   "100 Mad",  "70M+30C",    "50 Mad"};
        int[]    iconIdx = { 0,            1,           2,           3,            4      }; // Icon_01..05
        Button[] buildBtns = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            float x = -180f + i * 90f;
            var btnGO = MakePanel(buildPanel, $"Btn_{bNames[i]}",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x, -10f), new Vector2(82f, 110f));
            var btnImg = btnGO.AddComponent<Image>();
            Sprite bs = sSmallBlueSq != null ? sSmallBlueSq : sBigBlueBtn;
            if (bs != null) { btnImg.sprite = bs; btnImg.type = Image.Type.Simple; btnImg.preserveAspect = false; }
            else btnImg.color = new Color(0.25f, 0.45f, 0.85f, 1f);
            var btn = btnGO.AddComponent<Button>();
            // icon – use the actual building sprite
            if (buildingSprites[i] != null)
            {
                var iconGO = MakePanel(btnGO, "Icon",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0, -28), new Vector2(60, 60));
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = buildingSprites[i];
                iconImg.preserveAspect = true;
            }
            // name label
            var nameLbl = MakeTMP(btnGO, "Name", bNames[i], 10,
                new Vector2(0, -28), new Vector2(80, 22));
            nameLbl.alignment = TextAlignmentOptions.Center;
            nameLbl.color = Color.white;
            // cost label
            var costLbl = MakeTMP(btnGO, "Cost", bCosts[i], 9,
                new Vector2(0, -43), new Vector2(80, 18));
            costLbl.alignment = TextAlignmentOptions.Center;
            costLbl.color = new Color(1f, 0.95f, 0.6f, 1f);
            buildBtns[i] = btn;
        }
        // Close button (X) – top-right corner of build panel
        var closeBtnGO = MakePanel(buildPanel, "CloseBtn",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-16, -16), new Vector2(28, 28));
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        if (sTinyRedRound != null) { closeBtnImg.sprite = sTinyRedRound; closeBtnImg.preserveAspect = false; }
        else closeBtnImg.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        var closeBtnComp = closeBtnGO.AddComponent<Button>();
        var closeX = MakeTMP(closeBtnGO, "X", "✕", 14, Vector2.zero, new Vector2(26, 26));
        closeX.alignment = TextAlignmentOptions.Center;
        closeX.color = Color.white;
        buildPanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // 5. GAME OVER PANEL – fullscreen overlay, starts hidden
        // ════════════════════════════════════════════════════════════════════
        // ════════════════════════════════════════════════════════════════════
        var gameOverPanel = MakePanel(canvasGO, "GameOverPanel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetStretch(gameOverPanel);
        var goImg = gameOverPanel.AddComponent<Image>();
        goImg.color = new Color(0f, 0f, 0f, 0.78f);
        // decorative ribbon banner
        if (sBigRibbon1 != null)
        {
            var ribbonGO = MakePanel(gameOverPanel, "Ribbon",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
                Vector2.zero, new Vector2(500, 120));
            var rImg = ribbonGO.AddComponent<Image>();
            rImg.sprite = sBigRibbon1; rImg.preserveAspect = false;
        }
        var goTitle = MakeTMP(gameOverPanel, "Title", "GAME OVER", 64,
            new Vector2(0, 40), new Vector2(700, 100));
        goTitle.alignment = TextAlignmentOptions.Center;
        goTitle.color = new Color(0.95f, 0.15f, 0.1f, 1f);
        goTitle.fontStyle = FontStyles.Bold;
        var goSub = MakeTMP(gameOverPanel, "Subtitle", "Tu castillo ha sido destruido", 22,
            new Vector2(0, -20), new Vector2(600, 50));
        goSub.alignment = TextAlignmentOptions.Center;
        goSub.color = new Color(1f, 0.9f, 0.8f, 1f);
        // Restart hint
        var goHint = MakeTMP(gameOverPanel, "Hint", "Presiona ESC para salir", 16,
            new Vector2(0, -80), new Vector2(500, 30));
        goHint.alignment = TextAlignmentOptions.Center;
        goHint.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
        gameOverPanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // 7. PAUSE PANEL – fullscreen overlay, starts hidden
        // ════════════════════════════════════════════════════════════════════
        var pausePanel = MakePanel(canvasGO, "PausePanel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        SetStretch(pausePanel);
        var ppImg = pausePanel.AddComponent<Image>();
        ppImg.color = new Color(0f, 0f, 0f, 0.55f);
        var ppTitle = MakeTMP(pausePanel, "PauseText", "PAUSADO", 52,
            new Vector2(0, 20), new Vector2(400, 80));
        ppTitle.alignment = TextAlignmentOptions.Center;
        ppTitle.color = Color.white;
        ppTitle.fontStyle = FontStyles.Bold;
        pausePanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // 8. BUILDING INFO PANEL – right side, starts hidden
        // ════════════════════════════════════════════════════════════════════
        var bldgInfoPanel = MakePanel(canvasGO, "BuildingInfoPanel",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-90, 20), new Vector2(170, 130));
        var biImg = bldgInfoPanel.AddComponent<Image>();
        if (sRegularPaper != null) { biImg.sprite = sRegularPaper; biImg.preserveAspect = false; }
        else biImg.color = new Color(0.88f, 0.82f, 0.65f, 0.97f);
        var biTitle = MakeTMP(bldgInfoPanel, "Title", "Edificio Nv.1", 14,
            new Vector2(0, 38), new Vector2(160, 24));
        biTitle.alignment = TextAlignmentOptions.Center;
        biTitle.color = new Color(0.2f, 0.1f, 0f, 1f);
        biTitle.fontStyle = FontStyles.Bold;
        var biHint = MakeTMP(bldgInfoPanel, "Hint", "Pulsa Mejorar para\nsubir nivel (100💰)", 11,
            new Vector2(0, -5), new Vector2(160, 50));
        biHint.alignment = TextAlignmentOptions.Center;
        biHint.color = new Color(0.25f, 0.12f, 0f, 1f);
        bldgInfoPanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // 9. PAWN INFO PANEL – right side below bldg, starts hidden
        // ════════════════════════════════════════════════════════════════════
        var pawnInfoPanel = MakePanel(canvasGO, "PawnInfoPanel",
            new Vector2(1, 0.35f), new Vector2(1, 0.35f),
            new Vector2(-85, 0), new Vector2(160, 80));
        var piImg = pawnInfoPanel.AddComponent<Image>();
        if (sRegularPaper != null) { piImg.sprite = sRegularPaper; piImg.preserveAspect = false; }
        else piImg.color = new Color(0.88f, 0.82f, 0.65f, 0.97f);
        var piTitle = MakeTMP(pawnInfoPanel, "Title", "Peon", 14,
            new Vector2(0, 18), new Vector2(150, 24));
        piTitle.alignment = TextAlignmentOptions.Center;
        piTitle.color = new Color(0.2f, 0.1f, 0f, 1f);
        piTitle.fontStyle = FontStyles.Bold;
        pawnInfoPanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        // Wire all references onto GameHUD via SerializedObject
        // ════════════════════════════════════════════════════════════════════

        // -- Panel base class fields (UIManager) --
        so.FindProperty("pausePanel").objectReferenceValue          = pausePanel;
        so.FindProperty("gameOverPanel").objectReferenceValue       = gameOverPanel;
        so.FindProperty("buildPanel").objectReferenceValue          = buildPanel;
        so.FindProperty("pawnInfoPanel").objectReferenceValue       = pawnInfoPanel;
        so.FindProperty("buildingInfoPanel").objectReferenceValue   = bldgInfoPanel;

        // -- Castle HP fill image --
        so.FindProperty("castleHPFill").objectReferenceValue        = hpFillImg;

        // -- Resource texts (reuse the existing ResourcePanel Count TMP objects) --
        var woodCount  = GameObject.Find("HUD_Canvas/ResourcePanel/Wood/Count");
        var goldCount  = GameObject.Find("HUD_Canvas/ResourcePanel/Gold/Count");
        var foodCount  = GameObject.Find("HUD_Canvas/ResourcePanel/Food/Count");
        if (woodCount) so.FindProperty("woodText").objectReferenceValue = woodCount.GetComponent<TextMeshProUGUI>();
        if (goldCount) so.FindProperty("goldText").objectReferenceValue = goldCount.GetComponent<TextMeshProUGUI>();
        if (foodCount) so.FindProperty("foodText").objectReferenceValue = foodCount.GetComponent<TextMeshProUGUI>();
        // Auto-size resource text so 3-digit numbers fit cleanly
        void FixTMP(GameObject go) {
            if (go == null) return;
            var t = go.GetComponent<TextMeshProUGUI>();
            if (t == null) return;
            t.enableAutoSizing = true;
            t.fontSizeMin = 7f;
            t.fontSizeMax = 22f;
            t.overflowMode = TextOverflowModes.Overflow;
            t.fontStyle = FontStyles.Bold;
        }
        FixTMP(woodCount); FixTMP(goldCount); FixTMP(foodCount);

        // -- Timer --
        so.FindProperty("timerText").objectReferenceValue           = timerTMP;

        // -- Build buttons --
        so.FindProperty("barracksBtn").objectReferenceValue         = buildBtns[0];
        so.FindProperty("archeryBtn").objectReferenceValue          = buildBtns[1];
        so.FindProperty("towerBtn").objectReferenceValue            = buildBtns[2];
        so.FindProperty("monasteryBtn").objectReferenceValue        = buildBtns[3];
        so.FindProperty("houseBtn").objectReferenceValue            = buildBtns[4];

        // -- Wave banner --
        so.FindProperty("waveBannerRoot").objectReferenceValue      = waveBannerRoot;
        so.FindProperty("waveBannerText").objectReferenceValue      = waveBannerTMP;
        so.FindProperty("waveCountdownText").objectReferenceValue   = waveCountTMP;

        // -- Close build panel button --
        so.FindProperty("closeBuildPanelBtn").objectReferenceValue  = closeBtnComp;

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[UIBuilder] HUD built and wired successfully! Save the scene (Ctrl+S).");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Sprite SP(string sub) =>
        AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Tiny Swords/UI Elements/{sub}");

    static GameObject MakePanel(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = ancMin;
        rt.anchorMax        = ancMax;
        rt.anchoredPosition = ancPos;
        rt.sizeDelta        = size;
        return go;
    }

    static void SetStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text,
        float fontSize, Vector2 ancPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = ancPos;
        rt.sizeDelta        = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        return tmp;
    }
}
#endif
