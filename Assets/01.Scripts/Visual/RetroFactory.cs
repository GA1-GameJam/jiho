using UnityEngine;

public sealed class RetroFactory : MonoBehaviour
{
    [SerializeField] private float _balloonSpacing = 0.56f;
    [SerializeField] private float _balloonHeight = 1.02f;
    [SerializeField] private float _balloonRadius = 0.34f;
    [SerializeField] private Vector2 _balloonColliderOffset = new(0f, 0.28f);
    [SerializeField] private Vector2 _ropeSize = new(0.035f, 0.8f);
    [SerializeField] private float _ropeHorizontalFactor = 0.55f;
    [SerializeField] private float _ropeHeight = 0.55f;
    [SerializeField] private float _ropeAngleFactor = 28f;
    [SerializeField] private Color _ropeColor = new Color32(231, 226, 193, 255);
    [SerializeField] private int _ropeOrder = 8;
    [SerializeField] private int _balloonOrder = 9;
    [SerializeField] private int _actorOrder = 10;
    [SerializeField] private Color _playerBody = new Color32(42, 117, 215, 255);
    [SerializeField] private Color _playerHelmet = new Color32(232, 55, 55, 255);
    [SerializeField] private Color _playerSkin = new Color32(249, 207, 153, 255);
    [SerializeField] private Color _playerTwoBody = new Color32(50, 180, 105, 255);
    [SerializeField] private Color _playerTwoHelmet = new Color32(248, 205, 65, 255);
    [SerializeField] private Color _playerTwoSkin = new Color32(249, 207, 153, 255);
    [SerializeField] private Color _enemyABody = new Color32(134, 68, 179, 255);
    [SerializeField] private Color _enemyAHelmet = new Color32(240, 188, 65, 255);
    [SerializeField] private Color _enemyASkin = new Color32(166, 211, 102, 255);
    [SerializeField] private Color _enemyBBody = new Color32(63, 153, 129, 255);
    [SerializeField] private Color _enemyBHelmet = new Color32(215, 75, 126, 255);
    [SerializeField] private Color _enemyBSkin = new Color32(248, 194, 137, 255);
    [SerializeField] private Color _playerBalloonA = new Color32(237, 55, 72, 255);
    [SerializeField] private Color _playerBalloonB = new Color32(250, 190, 54, 255);
    [SerializeField] private Color _playerTwoBalloonA = new Color32(65, 205, 245, 255);
    [SerializeField] private Color _playerTwoBalloonB = new Color32(120, 235, 130, 255);
    [SerializeField] private Color _enemyBalloon = new Color32(214, 82, 177, 255);
    [SerializeField] private Sprite _playerSprite;
    [SerializeField] private Sprite _playerTwoSprite;
    [SerializeField] private Sprite _enemyASprite;
    [SerializeField] private Sprite _enemyBSprite;
    [SerializeField] private Sprite _playerBalloonASprite;
    [SerializeField] private Sprite _playerBalloonBSprite;
    [SerializeField] private Sprite _playerTwoBalloonASprite;
    [SerializeField] private Sprite _playerTwoBalloonBSprite;
    [SerializeField] private Sprite _enemyBalloonSprite;

    private static RetroFactory _config;
    private float BalloonSpacing => _balloonSpacing;
    private float BalloonHeight => _balloonHeight;
    private float BalloonRadius => _balloonRadius;
    private Vector2 BalloonColliderOffset => _balloonColliderOffset;
    private Vector2 RopeSize => _ropeSize;
    private float RopeHorizontalFactor => _ropeHorizontalFactor;
    private float RopeHeight => _ropeHeight;
    private float RopeAngleFactor => _ropeAngleFactor;
    private Color RopeColor => _ropeColor;
    private int RopeOrder => _ropeOrder;
    private int BalloonOrder => _balloonOrder;
    private int ActorOrder => _actorOrder;
    private Color PlayerBody => _playerBody;
    private Color PlayerHelmet => _playerHelmet;
    private Color PlayerSkin => _playerSkin;
    private Color PlayerTwoBody => _playerTwoBody;
    private Color PlayerTwoHelmet => _playerTwoHelmet;
    private Color PlayerTwoSkin => _playerTwoSkin;
    private Color EnemyABody => _enemyABody;
    private Color EnemyAHelmet => _enemyAHelmet;
    private Color EnemyASkin => _enemyASkin;
    private Color EnemyBBody => _enemyBBody;
    private Color EnemyBHelmet => _enemyBHelmet;
    private Color EnemyBSkin => _enemyBSkin;
    private Color PlayerBalloonA => _playerBalloonA;
    private Color PlayerBalloonB => _playerBalloonB;
    private Color PlayerTwoBalloonA => _playerTwoBalloonA;
    private Color PlayerTwoBalloonB => _playerTwoBalloonB;
    private Color EnemyBalloon => _enemyBalloon;
    private Sprite PlayerSprite => _playerSprite;
    private Sprite PlayerTwoSprite => _playerTwoSprite;
    private Sprite EnemyASprite => _enemyASprite;
    private Sprite EnemyBSprite => _enemyBSprite;
    private Sprite PlayerBalloonASprite => _playerBalloonASprite;
    private Sprite PlayerBalloonBSprite => _playerBalloonBSprite;
    private Sprite PlayerTwoBalloonASprite => _playerTwoBalloonASprite;
    private Sprite PlayerTwoBalloonBSprite => _playerTwoBalloonBSprite;
    private Sprite EnemyBalloonSprite => _enemyBalloonSprite;

    internal static void Configure(RetroFactory config)
    {
        _config = config;
        _player = null;
        _playerTwo = null;
        _enemyA = null;
        _enemyB = null;
        _redBalloon = null;
        _yellowBalloon = null;
        _playerTwoBalloonASpriteCache = null;
        _playerTwoBalloonBSpriteCache = null;
        _pinkBalloon = null;
    }

    private static Sprite _square;
    private static Sprite _player;
    private static Sprite _playerTwo;
    private static Sprite _enemyA;
    private static Sprite _enemyB;
    private static Sprite _redBalloon;
    private static Sprite _yellowBalloon;
    private static Sprite _playerTwoBalloonASpriteCache;
    private static Sprite _playerTwoBalloonBSpriteCache;
    private static Sprite _pinkBalloon;

    internal static GameObject CreateBlock(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject block = new(objectName);
        block.transform.SetParent(parent);
        block.transform.position = new Vector3(position.x, position.y, 0f);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSquare();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return block;
    }

    internal static void CreateFighter(Transform root, bool isPlayer, int balloonCount, int variation)
    {
        GameObject visual = new("Visual");
        visual.transform.SetParent(root);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = isPlayer ? GetPlayerSprite(variation) : GetEnemySprite(variation);
        visualRenderer.sortingOrder = _config.ActorOrder;

        for (int index = 0; index < balloonCount; index++)
        {
            float x = (index - (balloonCount - 1) / 2f) * _config.BalloonSpacing;
            CreateRope(root, x);
            CreateBalloon(root, isPlayer, index, x, variation);
        }
    }

    private static void CreateRope(Transform root, float x)
    {
        GameObject rope = CreateBlock(root, "Rope", Vector2.zero, _config.RopeSize, _config.RopeColor, _config.RopeOrder);
        rope.transform.localPosition = new Vector3(x * _config.RopeHorizontalFactor, _config.RopeHeight, 0f);
        rope.transform.localRotation = Quaternion.Euler(0f, 0f, -x * _config.RopeAngleFactor);
    }

    private static void CreateBalloon(Transform root, bool isPlayer, int index, float x, int variation)
    {
        GameObject balloon = new($"Balloon {index + 1}");
        balloon.transform.SetParent(root);
        balloon.transform.localPosition = new Vector3(x, _config.BalloonHeight, 0f);

        SpriteRenderer renderer = balloon.AddComponent<SpriteRenderer>();
        renderer.sprite = isPlayer
            ? GetPlayerBalloon(index, variation)
            : GetPinkBalloon();
        renderer.sortingOrder = _config.BalloonOrder;

        CircleCollider2D collider = balloon.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = _config.BalloonRadius;
        collider.offset = _config.BalloonColliderOffset;
        balloon.AddComponent<BalloonHitTarget>();
    }

    internal static Sprite GetSquare()
    {
        if (_square != null)
        {
            return _square;
        }

        Texture2D texture = CreateTexture(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        _square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _square;
    }

    private static Sprite GetPlayerSprite(int variation)
    {
        if (variation == 0)
        {
            if (_config.PlayerSprite != null) return _config.PlayerSprite;
            return _player ??= CreateFighterSprite(_config.PlayerBody, _config.PlayerHelmet, _config.PlayerSkin);
        }

        if (_config.PlayerTwoSprite != null) return _config.PlayerTwoSprite;
        return _playerTwo ??= CreateFighterSprite(
            _config.PlayerTwoBody,
            _config.PlayerTwoHelmet,
            _config.PlayerTwoSkin);
    }

    private static Sprite GetPlayerBalloon(int balloonIndex, int variation)
    {
        if (variation == 0)
        {
            return balloonIndex == 0 ? GetRedBalloon() : GetYellowBalloon();
        }

        if (balloonIndex == 0)
        {
            if (_config.PlayerTwoBalloonASprite != null) return _config.PlayerTwoBalloonASprite;
            return _playerTwoBalloonASpriteCache ??= CreateBalloonSprite(_config.PlayerTwoBalloonA);
        }

        if (_config.PlayerTwoBalloonBSprite != null) return _config.PlayerTwoBalloonBSprite;
        return _playerTwoBalloonBSpriteCache ??= CreateBalloonSprite(_config.PlayerTwoBalloonB);
    }

    private static Sprite GetEnemySprite(int variation)
    {
        Sprite supplied = variation % 2 == 0 ? _config.EnemyASprite : _config.EnemyBSprite;
        if (supplied != null) return supplied;
        if (variation % 2 == 0)
        {
            return _enemyA ??= CreateFighterSprite(_config.EnemyABody, _config.EnemyAHelmet, _config.EnemyASkin);
        }
        return _enemyB ??= CreateFighterSprite(_config.EnemyBBody, _config.EnemyBHelmet, _config.EnemyBSkin);
    }

    private static Sprite GetRedBalloon()
    {
        if (_config.PlayerBalloonASprite != null) return _config.PlayerBalloonASprite;
        return _redBalloon ??= CreateBalloonSprite(_config.PlayerBalloonA);
    }

    private static Sprite GetYellowBalloon()
    {
        if (_config.PlayerBalloonBSprite != null) return _config.PlayerBalloonBSprite;
        return _yellowBalloon ??= CreateBalloonSprite(_config.PlayerBalloonB);
    }

    private static Sprite GetPinkBalloon()
    {
        if (_config.EnemyBalloonSprite != null) return _config.EnemyBalloonSprite;
        return _pinkBalloon ??= CreateBalloonSprite(_config.EnemyBalloon);
    }

    private static Sprite CreateFighterSprite(Color body, Color helmet, Color skin)
    {
        Texture2D texture = CreateTexture(16, 16);
        Color outline = new Color32(18, 25, 39, 255);
        Color white = new Color32(245, 245, 230, 255);

        Fill(texture, 5, 11, 6, 3, helmet);
        Fill(texture, 4, 10, 8, 2, outline);
        Fill(texture, 5, 9, 6, 2, skin);
        Fill(texture, 6, 9, 1, 1, outline);
        Fill(texture, 9, 9, 1, 1, outline);
        Fill(texture, 5, 4, 6, 5, body);
        Fill(texture, 3, 5, 2, 3, white);
        Fill(texture, 11, 5, 2, 3, white);
        Fill(texture, 4, 2, 3, 2, outline);
        Fill(texture, 9, 2, 3, 2, outline);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.42f), 16f);
    }

    private static Sprite CreateBalloonSprite(Color mainColor)
    {
        Texture2D texture = CreateTexture(12, 16);
        Color outline = new Color32(23, 27, 42, 255);
        Color highlight = Color.Lerp(mainColor, Color.white, 0.45f);

        for (int y = 2; y < 15; y++)
        {
            for (int x = 1; x < 11; x++)
            {
                float normalizedX = (x - 5.5f) / 4.5f;
                float normalizedY = (y - 8.5f) / 6.2f;
                float ellipse = normalizedX * normalizedX + normalizedY * normalizedY;
                if (ellipse <= 1f)
                {
                    texture.SetPixel(x, y, ellipse > 0.72f ? outline : mainColor);
                }
            }
        }

        Fill(texture, 3, 4, 2, 4, highlight);
        Fill(texture, 5, 0, 2, 2, outline);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 12, 16), new Vector2(0.5f, 0.12f), 16f);
    }

    private static Texture2D CreateTexture(int width, int height)
    {
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int index = 0; index < pixels.Length; index++)
        {
            pixels[index] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void Fill(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int pixelY = y; pixelY < y + height; pixelY++)
        {
            for (int pixelX = x; pixelX < x + width; pixelX++)
            {
                if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                {
                    texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
}