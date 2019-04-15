using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FMOD;
using System;

namespace game1_with_fmod_wrapper
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        FMOD.System fmod;
        ChannelGroup masterChannel;
        ChannelGroup world3DChannel;
        uint raHandle;
        uint ralistenerHandle;
        uint raSourceHandle;
        DSP listenerDSP;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            FMOD.Factory.System_Create(out fmod);
            fmod.init(1024, INITFLAGS.NORMAL,(IntPtr)OUTPUTTYPE.AUTODETECT);
            fmod.getMasterChannelGroup(out masterChannel);
            fmod.createChannelGroup("world", out world3DChannel);
            fmod.loadPlugin("plugins/resonanceaudio.dll", out raHandle);
            fmod.getNestedPlugin(raHandle, 0, out ralistenerHandle);
            fmod.getNestedPlugin(raHandle, 2, out raSourceHandle);
            fmod.createDSPByPlugin(ralistenerHandle, out listenerDSP);
            world3DChannel.addDSP(world3DChannel.getNumDSPs+1,listenerDSP);

            


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Sound jumpsound;
            fmod.createSound("jump.wav", MODE.LOOP_NORMAL, out jumpsound);
            Channel jumpchannel;
            fmod.playSound(jumpsound, masterChannel,paused: false,channel: out jumpchannel);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            fmod.release();

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            fmod.update();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
