using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using TMPro;
using MoreRushes.Rush;

namespace MoreRushes.UI
{
    internal sealed class RushText : MonoBehaviour
    {
        private static RushText _instance;

        private Canvas _canvas;

        private TextMeshProUGUI _rushTmp;
        private TextMeshProUGUI _seedTmp;

        private static TMP_FontAsset _rushFont;
        private static TMP_FontAsset _seedFont;
        private static bool _fontsCached;

        private const float PaddingX = 15f;
        private const float PaddingY = 7.5f;

        private const string RushTextFontName = "RIFTON-CAPS SDF";
        private const float RushTextOutlineWidth = .32f;

        private const string SeedTextFontName = "SourceCodePro-Black SDF";
        private const float SeedTextOutlineWidth = .16f;

        private static readonly Dictionary<RushTextAnchor, (Vector2 anchorPos, Vector2 pivot, Vector2 textOffset, TextAlignmentOptions rushAlign, TextAlignmentOptions seedAlign)> anchorSettings = new()
        {
            { RushTextAnchor.TopLeft, (new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(PaddingX, -PaddingY), TextAlignmentOptions.TopLeft, TextAlignmentOptions.Left) },
            { RushTextAnchor.TopCenter, (new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -PaddingY), TextAlignmentOptions.Top, TextAlignmentOptions.Center) },
            { RushTextAnchor.TopRight, (new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-PaddingX, -PaddingY), TextAlignmentOptions.TopRight, TextAlignmentOptions.Right) },
            { RushTextAnchor.BottomLeft, (new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(PaddingX, PaddingY), TextAlignmentOptions.BottomLeft, TextAlignmentOptions.Left) },
            { RushTextAnchor.BottomRight, (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-PaddingX, PaddingY), TextAlignmentOptions.BottomRight, TextAlignmentOptions.Right) }
        };

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public static void EnsureCreated()
        {
            if (_instance == null)
            {
                var rushTextObj = new GameObject($"{MoreRushes.ModInstance.Info.Name}.{nameof(RushText)}");
                rushTextObj.AddComponent<RushText>();
            }
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
                Destroy(_instance.gameObject);
        }

        private void CreateUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999; // below pointer

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            EnsureFonts();

            _rushTmp = CreateTextObject("RushText", _rushFont, RushTextOutlineWidth, 
                RushToColor(RushManager.ActiveRush), .04f, new Vector2(2f, -2f));
            _seedTmp = CreateTextObject("SeedText", _seedFont, SeedTextOutlineWidth,
                Settings.seedTextColorEntry.Value, .03f, new Vector2(.5f, -.5f));

            var rushTextObj = new GameObject("RushText");
            rushTextObj.transform.SetParent(_canvas.transform, false);

            _rushTmp.gameObject.SetActive(!Settings.showSeedTextOnlyEntry.Value);

            UpdateRush();
            ApplyFontSizeSetting();
        }

        private TextMeshProUGUI CreateTextObject(
            string name, TMP_FontAsset font, float outlineWidth, Color32 textColor, float faceDilate, Vector2 shadowOffset)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(_canvas.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.enableVertexGradient = true;
            tmp.rectTransform.sizeDelta = new Vector2(700f, 0f);
            tmp.extraPadding = true;
            tmp.outlineWidth = outlineWidth;
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
            AddShadow(tmp, new Color(0f, 0f, 0f, .5f), shadowOffset, .2f);
            tmp.colorGradient = CreateGradient(textColor);
            tmp.outlineColor = DarkenColor(textColor, .3f);

            return tmp;
        }

        private static VertexGradient CreateGradient(Color32 baseColor)
        {
            var darkColor = DarkenColor(baseColor, .8f);
            return new VertexGradient(baseColor, baseColor, darkColor, darkColor);
        }

        private static void AddShadow(TextMeshProUGUI tmp, Color color, Vector2 offset, float softness)
        {
            if (tmp == null) return;

            tmp.fontMaterial.EnableKeyword("UNDERLAY_ON");
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, offset.x);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, offset.y);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, softness);
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, color);
        }

        private static Color DarkenColor(Color color, float darkenFactor = .8f)
        {
            darkenFactor = Mathf.Clamp01(darkenFactor);
            return new Color(color.r * darkenFactor, color.g * darkenFactor, color.b * darkenFactor, color.a);
        }

        private static Color32 RushToColor(RushMode mode)
        {
            return mode switch
            {
                RushMode.Elevate => Settings.elevateColorEntry.Value,
                RushMode.Godspeed => Settings.godspeedColorEntry.Value,
                RushMode.Stomp => Settings.stompColorEntry.Value,
                RushMode.Fireball => Settings.fireballColorEntry.Value,
                RushMode.Mikey => Settings.dominionColorEntry.Value,
                RushMode.Random => Settings.randomRushColorEntry.Value,
                _ => Settings.purifyColorEntry.Value,
            };
        }

        private static void EnsureFonts()
        {
            if (_fontsCached) return;

            foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (fontAsset.name == RushTextFontName) _rushFont = fontAsset;
                else if (fontAsset.name == SeedTextFontName) _seedFont = fontAsset;
            }

            if (_rushFont == null)
                MelonLogger.Warning($"Failed to find font for rush text: {RushTextFontName}");
            if (_seedFont == null)
                MelonLogger.Warning($"Failed to find font for rush seed text: {SeedTextFontName}");

            _fontsCached = true;
        }

        public static void UpdateRush()
        {
            if (_instance == null || _instance._rushTmp == null || _instance._seedTmp == null)
                return;

            _instance._rushTmp.text = $"{RushManager.ActiveRush} Rush";

            bool isRandomRush = RushManager.ActiveRush == RushMode.Random;
            _instance._seedTmp.gameObject.SetActive(isRandomRush);
            if (isRandomRush) _instance._seedTmp.text = $"seed: {RushManager.CurrentSeed}";

            var rushColor = RushToColor(RushManager.ActiveRush);
            _instance._rushTmp.colorGradient = CreateGradient(rushColor);
            _instance._rushTmp.outlineColor = DarkenColor(rushColor, .3f);

            var seedColor = Settings.seedTextColorEntry.Value;
            _instance._seedTmp.colorGradient = CreateGradient(seedColor);
            _instance._seedTmp.outlineColor = DarkenColor(seedColor, .3f);

            ApplyAnchorSetting();
        }

        public static void ApplyOnlyShowSeedTextSetting()
        {
            if (_instance == null || _instance._rushTmp == null || _instance._seedTmp == null)
                return;

            _instance._rushTmp.gameObject.SetActive(!Settings.showSeedTextOnlyEntry.Value);

            ApplyAnchorSetting();
        }

        public static void ApplyFontSizeSetting()
        {
            if (_instance == null || _instance._rushTmp == null || _instance._seedTmp == null)
                return;

            var fontSize = Settings.rushTextSizeEntry.Value;

            _instance._rushTmp.fontSize = fontSize;
            _instance._seedTmp.fontSize = fontSize * .9f;

            ApplyAnchorSetting();
        }

        public static void ApplyAnchorSetting()
        {
            if (_instance == null || _instance._rushTmp == null || _instance._seedTmp == null)
                return;

            var anchor = Settings.rushTextAnchorEntry.Value;
            var rushRect = _instance._rushTmp.rectTransform;
            var seedRect = _instance._seedTmp.rectTransform;

            const float BaseFontSize = 38f;
            const float BasePixelGap = 10f;
            float pixelGap = BasePixelGap * (_instance._rushTmp.fontSize / BaseFontSize);

            var (anchorPos, pivot, textOffset, rushAlignment, seedAlignment) = anchorSettings[anchor];

            if (!Settings.showSeedTextOnlyEntry.Value)
            {
                rushRect.anchorMin = rushRect.anchorMax = anchorPos;
                rushRect.pivot = pivot;
                rushRect.anchoredPosition = textOffset;
                _instance._rushTmp.alignment = rushAlignment;

                seedRect.anchorMin = seedRect.anchorMax = anchorPos;
                seedRect.pivot = pivot;
                _instance._seedTmp.alignment = seedAlignment;
            }
            else
            {
                seedRect.anchorMin = seedRect.anchorMax = anchorPos;
                seedRect.pivot = pivot;
                seedRect.anchoredPosition = textOffset;
                _instance._seedTmp.alignment = rushAlignment;
                return;
            }

            _instance._rushTmp.ForceMeshUpdate();
            _instance._seedTmp.ForceMeshUpdate();

            float rushHeight = _instance._rushTmp.textBounds.size.y;
            float seedHeight = _instance._seedTmp.textBounds.size.y;
            float totalOffset = (rushHeight * 0.5f) + pixelGap + (seedHeight * 0.5f);
            Vector2 direction = anchorPos.y > 0.5f ? Vector2.down : Vector2.up;

            seedRect.anchoredPosition = textOffset + direction * totalOffset;
        }
    }
}
