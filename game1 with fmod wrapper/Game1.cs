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
        VECTOR listenerPos = new VECTOR { x = 10, y = 0, z = -5 };
        VECTOR listenerVel = new VECTOR { x = 0, y = 0, z = 0 };
        VECTOR listenerForward = new VECTOR { x = 0, y = 0, z = 1 };
        VECTOR listenerUp = new VECTOR { x = 0, y = 1, z = 0 };

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
            errcheck(FMOD.Factory.System_Create(out fmod));
            errcheck(fmod.init(1024, INITFLAGS.NORMAL, (IntPtr)OUTPUTTYPE.AUTODETECT));
            errcheck(fmod.getMasterChannelGroup(out masterChannel));
            errcheck(fmod.createChannelGroup("world", out world3DChannel));
            errcheck(fmod.loadPlugin("plugins/resonanceaudio.dll", out raHandle));

            errcheck(fmod.getNestedPlugin(raHandle, 0, out ralistenerHandle));
            errcheck(fmod.getNestedPlugin(raHandle, 2, out raSourceHandle));
            errcheck(fmod.createDSPByPlugin(ralistenerHandle, out listenerDSP));
            errcheck(world3DChannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, listenerDSP));
            errcheck(fmod.set3DNumListeners(1));
            errcheck(fmod.set3DListenerAttributes(0, ref listenerPos, ref listenerVel, ref listenerForward, ref listenerUp));


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
            errcheck(fmod.createSound("jump.wav", MODE._3D | MODE.LOOP_NORMAL, out jumpsound));
            Channel jumpchannel;
            errcheck(fmod.playSound(jumpsound, world3DChannel, paused: true, channel: out jumpchannel));
            errcheck(jumpchannel.setMode(MODE._3D));
            errcheck(fmod.createDSPByPlugin(raSourceHandle, out DSP sourceDSP));
            errcheck(jumpchannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, sourceDSP));
            
            VECTOR pos = new VECTOR { x = 30, y = 0, z = 0 };
            VECTOR vel = new VECTOR { x = 0, y = 0, z = 0 };
            errcheck(jumpchannel.set3DAttributes(ref pos, ref vel));
            errcheck(jumpchannel.setPaused(false));






            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            errcheck(fmod.release());

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
            errcheck(fmod.update());
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

        private void errcheck(RESULT fmodresult)
        {
            
            if(fmodresult != RESULT.OK)
            {
                Console.WriteLine($"Error de fmod durante la ejecución {fmodresult}. {Error.String(fmodresult) } botando programa");
                throw new Exception("Caída por error de fmod.");
            }
            else
            {
                Console.WriteLine(" bien ");
            }
        }
    }
}

