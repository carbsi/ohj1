using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;


/*
Silmukka:
https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo5?answerNumber=1&task=lukujenTulostusT2&user=karpot
Taulukko:
https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo6?answerNumber=15&task=TeeTaulukko&user=karpot
Funktio:
https://tim.jyu.fi/answers/kurssit/tie/ohj1/2022k/demot/demo5?answerNumber=4&task=summaylid5&user=karpot
*/
 /// @author Otto Karppinen
 /// @version 24052022
 /// @RobininhyppelyPeli
 /// <summary>
 /// Moni ei tiedä että Batmanin Robin rakastaa tomaattimurskaa. Pelissä on tarkoitus kerätä kaikki tomaatit
 /// jolloin peli päättyy. Tasohyppelykentällä liikkuu vihollisia joihin ei saa osua. Pelissä on myös aikaraja jolloin peli päättyy. Peli loppuu joko keräämällä kaikki
 /// objektit tai osumalla viholliseen tai ajastimeen.
 /// </summary>
 

public class Harjoitustyo : PhysicsGame
{
	/// <summary>
    /// Ladataan pelin käynnistys
	/// </summary>
    private bool peliKaynnissa = true;
	
	
	/// <summary>
    /// Ladataan pelaajan liikkumisnopeus
	/// </summary>
    private const double NOPEUS = 200;
	
	
	/// <summary>
    /// Ladataan pelaajan hyppynopeus
	/// </summary>
    private const double HYPPYNOPEUS = 750;
	
	
	/// <summary>
    /// Ladataan pelimaailman reunukset
	/// </summary>
    private const int RUUDUN_KOKO = 40;
	
	
	/// <summary>
    /// Ladataan itse pelaaja
	/// </summary>
    private PlatformCharacter pelaaja1;
	
	
	/// <summary>
    /// Ladataan pelaajan kuva
	/// </summary>
    private readonly Image pelaajanKuva = LoadImage("robin.png");
	
	
	/// <summary>
    /// Ladataan kerättävien objektien kuvat
	/// </summary>
    private readonly Image tomaattiKuva = LoadImage("tomaatti.png");
	
	
	/// <summary>
    /// Ladataan vihollisen kuva
	/// </summary>
    private readonly Image vihuKuva = LoadImage("bane.png");
	

	/// <summary>
	/// Ladataan ääniefektit, kun osutaan viholliseen tai kerättäviin objekteihin
	/// </summary>
    private SoundEffect kerasTomaatin = LoadSoundEffect("yes.wav");
	
	
	/// <summary>
    /// Ladataan ääniefekti kun pelimaailma loppuu
	/// </summary>
    private SoundEffect bye = LoadSoundEffect("bye.wav");
	
	
	/// <summary>
    /// Ladataan ääniefekti joka räjäyttää pelimaailman
	/// </summary>
    private SoundEffect rajahdys = LoadSoundEffect("explosion.wav");
	
	
	/// <summary>
    /// Ladataan ääniefekti joka kun pelaaja osuu vihuun
	/// </summary>
    private SoundEffect isku = LoadSoundEffect("punch.wav");
	
	
	/// <summary>
    /// Ladataan ääniefekti kun pelaaja osuu vihuun ja kuolee
	/// </summary>
    private SoundEffect kituminen = LoadSoundEffect("manDying.wav");
	
	
	/// <summary>
    /// Ladataan ääniefekti, jossa peli loppuu voittoon
	/// </summary>
    private SoundEffect winning = LoadSoundEffect("winn.wav");
	
	
	/// <summary>
    /// Ladataan pisteiden laskemiisen eli tomaattien keräämiseen oleva mittari
	/// </summary>
    private IntMeter pisteLaskuri;
	

	/// <summary>
	/// Ladataan pelin aloitus
	/// </summary>
    public override void Begin()
    {
        AloitaPeli();
    }
	

	/// <summary>
	/// Aliohjelma, jossa kutsutaan kaikkea pelin aloittamiseen tarvittavat
	/// </summary>
    private void AloitaPeli()
    {
        MessageDisplay.Add("45-sekunttia aikaa ennen kuin Bane räjäyttää maailman, kerää kaikki tomaattimurskat ennen sitä!");
        LuoPistelaskuri();
        Gravity = new Vector(0, -1000);
        LuoKentta();
        LisaaNappaimet();
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
        LuoAikaLaskuri();
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Start(1);
        Label aikaNaytto = new Label();
        aikaNaytto.X = Screen.Left + 1000;
        aikaNaytto.Y = Screen.Bottom + 750;
        aikaNaytto.TextColor = Color.Black;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        Add(aikaNaytto);
    }
	

	/// <summary>
	/// Luodaan kenttä ja metodit joilla kenttää voidaan muokata
	/// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaTomaatti);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.SetTileMethod('V', LisaaLiikkuvaVihu);
        kentta.SetTileMethod('A', LisaaVihu);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }
	

	/// <summary>
	/// Lisätään palikat, jotka muodostavat hyppimiskentän
	/// </summary>
	/// <param name="paikka">Tason paikka kentällä</param>
	/// <param name="leveys">Tason leveys</param>
	/// <param name="korkeus">Tason korkeus</param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Green;
        Add(taso);
    }
	

	/// <summary>
	/// Aliohjelma, jolla lisätään kerättävät objektit
	/// </summary>
	/// <param name="paikka">Objektin paikka kentällä</param>
	/// <param name="leveys">Objektin leveys</param>
	/// <param name="korkeus">Objektin korkeus</param>
    private void LisaaTomaatti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tomaatti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tomaatti.IgnoresCollisionResponse = true;
        tomaatti.Position = paikka;
        tomaatti.Image = tomaattiKuva;
        tomaatti.Tag = "tomaatti";
        Add(tomaatti);
    }
	

	/// <summary>
	/// Aliohjelma, jolla lisätään pelaaja
	/// </summary>
	/// <param name="paikka">pelaajan paikka kentällä aloittaessa</param>
	/// <param name="leveys">pelaajan leveys</param>
	/// <param name="korkeus">pelaajan korkeus</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "tomaatti", TormaaTomaattiin);
        AddCollisionHandler(pelaaja1, "vihu", TormaaVihuun);
        Add(pelaaja1);
    }
	

	/// <summary>
	/// Aliohjelma painikkeiden lisäämiseen.
	/// </summary>
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1, -NOPEUS);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        Keyboard.Listen(Key.F2, ButtonState.Pressed, AloitaPeli, "Aloittaa uuden pelin");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
	

	/// <summary>
	/// Aliohjelma, joka lisää viholliset peliin. Luodaan aivot, jotka antavat liikkumisominaisuuden
	/// </summary>
	/// <param name="paikka">Liikkuvan vihun paikka kentällä</param>
	/// <param name="leveys">Liikkuvan vihun leveys</param>
	/// <param name="korkeus">Liikkuvan vihun korkeus</param>
    private void LisaaLiikkuvaVihu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vihu = new PlatformCharacter(40, 40);
        vihu.CollisionIgnoreGroup = 1;
        vihu.Position = paikka;
        vihu.Shape = Shape.Rectangle;
        vihu.Mass = 4.0;
        vihu.Image = vihuKuva;
        vihu.Tag = "vihu";
        Add(vihu);
        RandomMoverBrain satunnaisAivot = new RandomMoverBrain();
        vihu.Brain = satunnaisAivot;
        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.FallsOffPlatforms = true;
        tasoAivot.Speed = 40;
        vihu.Brain = tasoAivot;
    }
	

	/// <summary>
	/// Aliohjelma, joka lisää staattisen vihollisen peliin.
	/// </summary>
	/// <param name="paikka">Paikallaan olevan vihun lokaatio kentällä</param>
	/// <param name="leveys">Paikallaan olevan vihun leveys</param>
	/// <param name="korkeus">Paikallaan olevan vihun korkeus</param>
    private void LisaaVihu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vihu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        vihu.CollisionIgnoreGroup = 1;
        vihu.Position = paikka;
        vihu.Shape = Shape.Rectangle;
        vihu.Mass = 4.0;
        vihu.Image = vihuKuva;
        vihu.Tag = "vihu";
        Add(vihu);
    }
	

	/// <summary>
	/// Aliohjelma, joka luo pistelaskurin, jolla lasketaan kerätyt objektit. Asetetaan kerätyille objekteille maksimi, jolla voittaa pelin. Ohjelma kutsuu toista aliohjelmaa kun kaikki objektit on kerätty
	/// </summary>
    private void LuoPistelaskuri()
    {
        int tomaattienMaara = 33;
        pisteLaskuri = new IntMeter(0);
        pisteLaskuri.MaxValue = tomaattienMaara;
        pisteLaskuri.UpperLimit += KaikkiKeratty;
        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 150;
        pisteNaytto.Y = Screen.Bottom + 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Tomaatteja kerätty 33/: ";
        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }
	

	/// <summary>
	/// Aliohjelma, joka pysäyttää pelin ja näyttää tekstin, kun kaikki objektit on kerätty
	/// </summary>
    private void KaikkiKeratty()
    {  
        MessageDisplay.Add("Keräsit kaikki tomaatit, voitit pelin!");
        winning.Play();
        peliKaynnissa = false;
        IsPaused = true;
    }
	

	/// <summary>
	/// Jypelin Tasohyppelypohjasta valmiina otettu hahmon liikkumisominaisuus
	/// </summary>
	/// <param name="hahmo">Hahmo Jypelistä</param>
	/// <param name="nopeus">Hahmon nopeus</param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }
	

	/// <summary>
	/// Jypelin Tasohyppelypohjasta valmiina otettu hahmon hyppysominaisuus
	/// </summary>
	/// <param name="hahmo">Jypelin Hahmo</param>
	/// <param name="nopeus">Jypelin hahmon nopeus</param>
    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
	

	/// <summary>
	/// Aliohjelma, jolla määritellään mitä tapahtuu, kun osutaan kerättävään objektiin. Ääni, teksti, objektin katoaminen ja pisteiden kertyminen 
	/// </summary>
	/// <param name="hahmo">Hahmo Jypelistä</param>
	/// <param name="tomaatti">Jypelin ominaisuuksilla tomaatti</param>
    private void TormaaTomaattiin(PhysicsObject hahmo, PhysicsObject tomaatti)
    {
        kerasTomaatin.Play();
        MessageDisplay.Add("Keräsit tomaatin!");
        tomaatti.Destroy();
        pisteLaskuri.Value += 1;
    }
	

	/// <summary>
	/// Aliohjelma, jolla määritellään mitä tapahtuun, kun osutaan viholliseen. Ääniefektit, teksti, pelaajan katoaminen ja ajan pysähtyminen.
	/// </summary>
	/// <param name="hahmo">Hahmo Jypelistä</param>
	/// <param name="vihu">Vihollinen Jypelin ominaisuuksilla</param>
    private void TormaaVihuun(PhysicsObject hahmo, PhysicsObject vihu)
    {
        bye.Play();
        isku.Play();
        kituminen.Play();
        MessageDisplay.Add("Bane lahtas sinut, peli loppui");
        pelaaja1.Destroy();
        peliKaynnissa = false;
        IsPaused = true;
    }
	

	/// <summary>
	/// Aliohjelma, joka laskee aikaa kunnes pommi räjähtää, kutsuu toista aliohjelmaa kun aikaa on kulunut tarpeeksi
	/// </summary>
    private void LuoAikaLaskuri()
    {
        int aikaraja = 45;
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Interval = aikaraja;
        aikaLaskuri.Timeout += AikaLoppui;
        aikaLaskuri.Start(1);
    }
	

	/// <summary>
	/// Aliohjelma joka laukaisee räjähdyksen ja pysäytää pelin
	/// </summary>
    private void AikaLoppui()
    {
        MessageDisplay.Add("Aika loppui... Bane räjäytti maailman");
        bye.Play();
        rajahdys.Play();
        pelaaja1.Destroy();
        peliKaynnissa = false;
        IsPaused = true;
    }
}
