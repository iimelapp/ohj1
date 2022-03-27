using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Iina Lappalainen
/// @version 27.11.2016
public class Harjoitustyo : PhysicsGame
{
    private PhysicsObject kissa;
    private PhysicsObject olio;
    private readonly IntMeter pisteLaskuri = new IntMeter(0, 0, 15);
    private readonly IntMeter alaspainLaskuri = new IntMeter(30);
    private Timer aikaLaskuri;
    private Label pisteNaytto;
    private Label aikaNaytto;
    private Vector nopeus = new Vector(150, 0);
    private Vector hyppyNopeus = new Vector(0, 150);
    private double RUUDUN_KOKO = 40;
    private List<string> kentat = new List<string> { "kentta1", "kentta2", "kentta3" };
    private int kenttanro = 0;

    /// <summary>
    /// Aloitetaan peli
    /// </summary>
    public override void Begin()
    {
        LuoKentta(kentat[kenttanro]);
        LuoKissa(this, 20, 20);
        AsetaOhjaimet();
        LuoNaytot();
        KissaTormasi();
        Camera.Follow(kissa);
        Camera.StayInLevel = true;
        Camera.ZoomFactor = 2;
    }

    /// <summary>
    /// Luodaan kenttä ja lisätään oliot
    /// </summary>
    /// <param name="taso">Merkkijono, joka kertoo tasonnumeron joka luodaan</param>
    public void LuoKentta(string taso)
    {
        Gravity = new Vector(0, -500);
        TileMap kentta = TileMap.FromLevelAsset(taso);
        kentta.SetTileMethod('#', LuoTaso);
        kentta.SetTileMethod('p', LuoHiiri);
        kentta.SetTileMethod('s', LuoKala);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.Color = Color.LightBlue;
    }


    /// <summary>
    /// Määritellään näppäimet joilla pelissä voidaan liikkua ja jolla siitä poistutaan
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Liikuta, "Pallo liikkuu ylös", kissa, hyppyNopeus);
        Keyboard.Listen(Key.Left, ButtonState.Pressed, Liikuta, "Pallo liikkuu vasemmalle", kissa, -nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Pressed, Liikuta, "Pallo liikkuu oikealle", kissa, nopeus);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Määritellään millä vauhdilla fysiikkaoliota liikutetaan
    /// </summary>
    /// <param name="kissa">Olio, jota liikutellaan</param>
    /// <param name="nopeus">Vektori, joka kertoo liikkumisnopeuden</param>
    public void Liikuta(PhysicsObject kissa, Vector nopeus)
    {
        kissa.Hit(nopeus);
    }


    /// <summary>
    /// Luodaan pelaaja
    /// </summary>
    /// <param name="peli">Peli, jossa pelaaja on</param>
    /// <param name="x">Pelaajan x-koordinaatti pelin alussa</param>
    /// <param name="y">Pelaajan y-koordinaatti pelin alussa</param>
    /// <returns></returns>
    private PhysicsObject LuoKissa(PhysicsGame peli, double x, double y)
    {
        kissa = new PhysicsObject(40.0, 40.0);
        kissa.Color = Color.Transparent;
        kissa.Image = LoadImage("kissa");
        Add(kissa);
        kissa.X = 0.0;
        kissa.Y = Level.Bottom + 100;
        kissa.CanRotate = false;
        kissa.Restitution = 0;
        return kissa;
    }


    /// <summary>
    /// Luodaan kentälle olioita
    /// </summary>
    /// <param name="paikka">Vektori, joka kertoo olion paikan</param>
    /// <param name="w">Olion leveys</param>
    /// <param name="h">Olion korkeus</param>
    /// <param name="vari">Olion väri</param>
    /// <param name="tunniste">Olion tunniste</param>
    private void LisaaOlio(Vector paikka, double w, double h, Color vari, string tunniste)
    {
        olio = PhysicsObject.CreateStaticObject(w, h);
        olio.Position = paikka;
        olio.Color = vari;
        olio.Image = LoadImage(tunniste);
        olio.Tag = tunniste;
        olio.Restitution = 0;
        Add(olio);        
    }


    /// <summary>
    /// Luodaan taso
    /// </summary>
    /// <param name="paikka">Tason paikka</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    public void LuoTaso(Vector paikka, double leveys, double korkeus)
    {
        LisaaOlio(paikka, 100, 10, Color.Black, "taso");
    }


    /// <summary>
    /// Luodaan hiiri
    /// </summary>
    /// <param name="paikka">Hiiren paikka</param>
    /// <param name="leveys">Hiiren leveys</param>
    /// <param name="korkeus">Hiiren korkeus</param>
    public void LuoHiiri(Vector paikka, double leveys, double korkeus)
    {
        LisaaOlio(paikka, 80, 40, Color.Red, "hiiri");
    }


    /// <summary>
    /// Luodaan kala
    /// </summary>
    /// <param name="paikka">Kalan paikka</param>
    /// <param name="leveys">Kalan leveys</param>
    /// <param name="korkeus">Kalan korkeus</param>
    public void LuoKala(Vector paikka, double leveys, double korkeus)
    {
        LisaaOlio(paikka, 50, 50, Color.Blue, "kala");
    }


    /// <summary>
    /// Lisätään törmäyskäsittelijät, jotka poistavat törmäyksen kohteen ja lisäävät pistelaskurin arvoa
    /// </summary>
    public void KissaTormasi()
    {
        AddCollisionHandler(kissa, "hiiri", CollisionHandler.DestroyTarget);
        AddCollisionHandler(kissa, "kala", CollisionHandler.DestroyTarget);
        AddCollisionHandler(kissa, "hiiri", CollisionHandler.AddMeterValue(pisteLaskuri, 3));
        AddCollisionHandler(kissa, "kala", CollisionHandler.AddMeterValue(pisteLaskuri, 1));
    }


    /// <summary>
    /// Luodaan laskurit, jotka kertovat pistemäärän ja jäljellä olevan ajan
    /// </summary>
    public void LuoNaytot()
    {
        pisteNaytto = LuoNaytto("Pisteet: {0}", pisteLaskuri, 1);
        pisteLaskuri.UpperLimit += KaikkiKeratty;
        aikaNaytto = LuoNaytto("Aika: {0}", alaspainLaskuri, 2);
        aikaLaskuri = new Timer();
        aikaLaskuri.Interval = 1;
        aikaLaskuri.Timeout += LaskeAlaspain;
        aikaLaskuri.Start();
    }


    /// <summary>
    /// Luodaan näyttö, jossa on laskurin arvo
    /// </summary>
    /// <param name="title">Näytön otsikko</param>
    /// <param name="mittari">Laskuri, jonka arvon näyttö kertoo</param>
    /// <param name="n">Näytön numero</param>
    /// <returns>Tekstikenttä, jossa on laskurin arvo</returns>
    public Label LuoNaytto(string title, IntMeter mittari, int n)
    {
        Label naytto = new Label(title);
        naytto.IntFormatString = title;
        naytto.BindTo(mittari);
        naytto.X = Level.Right - 100;
        naytto.Y = Level.Top - 100 - n * 20;
        naytto.TextColor = Color.Black;
        Add(naytto);
        return naytto;
    }


    /// <summary>
    /// Määritellään mitä tapahtuu, kun on saatu riittävä määrä pisteitä
    /// </summary>
    public void KaikkiKeratty()
    {
        kenttanro++;
        SeuraavaKentta();
    }


    /// <summary>
    /// Luodaan aikalaskuri, joka lähenee nollaa
    /// </summary>
    public void LaskeAlaspain()
    {
        alaspainLaskuri.Value -= 1;
        if (alaspainLaskuri.Value <= 0)
        {
            ClearAll();
            AsetaOhjaimet();
            MessageDisplay.Add("Aika loppui");
            aikaLaskuri.Stop();
        }
    }


    /// <summary>
    /// Määritellään mitä tapahtuu, kun taso on läpäisty
    /// </summary>
    public void SeuraavaKentta()
    {
        while (kenttanro < 3)
        {
            ClearAll();
            alaspainLaskuri.Reset();
            pisteLaskuri.Reset();
            Begin();
            break;         
        }
        if (kenttanro > kentat.Count) Exit();
    }
}

