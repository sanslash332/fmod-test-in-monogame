using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FMOD;
using System;
using System.Runtime.InteropServices;
using System.Text;

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
        VECTOR listenerPos = new VECTOR { x = 0, y = 0, z = 0 };
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

            errcheck(fmod.createDSPByPlugin(raHandle, out DSP raDSP));
            StringBuilder radspinfo = new StringBuilder();

            //v2 errcheck(raDSP.getInfo(out string radspinfo, out uint radspversion, out int radspchannels, out int radspheight, out int radspwidth));
            errcheck(raDSP.getInfo(radspinfo, out uint radspversion, out int radspchannels, out int radspheight, out int radspwidth));
            Console.WriteLine($" detalles de resonanse audio dsp: {radspinfo} {radspversion} ");
            errcheck(fmod.getNestedPlugin(raHandle, 0, out ralistenerHandle));
            errcheck(fmod.getNestedPlugin(raHandle, 1, out uint rasoundfieldHandle));
            errcheck(fmod.getNestedPlugin(raHandle, 2, out raSourceHandle));
            errcheck(fmod.createDSPByPlugin(ralistenerHandle, out listenerDSP));
            StringBuilder listenerdspinfo = new StringBuilder();
            //v2 errcheck(listenerDSP.getInfo(out string listenerdspinfo, out uint listenerdspversion, out int listenerdspchannels, out int listenerdspheight, out int listenerdspwidth));
            errcheck(listenerDSP.getInfo(listenerdspinfo, out uint listenerdspversion, out int listenerdspchannels, out int listenerdspheight, out int listenerdspwidth));
            Console.WriteLine($" detalles de resonanse audio listener dsp: {listenerdspinfo} {listenerdspversion} ");
            errcheck(fmod.createDSPByPlugin(rasoundfieldHandle, out DSP soundfieldDSP));
            StringBuilder soundfielddspinfo = new StringBuilder();
            //v2 errcheck(soundfieldDSP.getInfo(out string soundfielddspinfo, out uint soundfielddspversion, out int soundfielddspchannels, out int soundfielddspheight, out int soundfielddspwidth));
            errcheck(soundfieldDSP.getInfo(soundfielddspinfo, out uint soundfielddspversion, out int soundfielddspchannels, out int soundfielddspheight, out int soundfielddspwidth));
            Console.WriteLine($" detalles de resonanse audio soundfield dsp: {soundfielddspinfo} {soundfielddspversion} ");
            errcheck(fmod.createDSPByPlugin(raSourceHandle, out DSP sourceDSP));
            StringBuilder sourcedspinfo = new StringBuilder();

            //v2 errcheck(sourceDSP.getInfo(out string sourcedspinfo, out uint sourcedspversion, out int sourcedspchannels, out int sourcedspheight, out int sourcedspwidth));
            errcheck(sourceDSP.getInfo(sourcedspinfo, out uint sourcedspversion, out int sourcedspchannels, out int sourcedspheight, out int sourcedspwidth));
            Console.WriteLine($" detalles de resonanse audio source dsp: {sourcedspinfo} {sourcedspversion}  {sourcedspchannels}");

            errcheck(masterChannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL,listenerDSP));
            //errcheck(world3DChannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, listenerDSP));
            //errcheck(fmod.set3DNumListeners(1));
            
            errcheck(fmod.set3DListenerAttributes(0, ref listenerPos, ref listenerVel, ref listenerForward, ref listenerUp));
            //errcheck(masterChannel.addGroup(world3DChannel));


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
            errcheck(fmod.createSound("jump.wav", MODE.LOOP_NORMAL, out jumpsound));
            Channel jumpchannel;
            errcheck(fmod.playSound(jumpsound, masterChannel, paused: true, channel: out jumpchannel));
            errcheck(jumpchannel.setMode(MODE._3D));
            errcheck(fmod.createDSPByPlugin(raSourceHandle, out DSP sourceDSP));
            errcheck(sourceDSP.getNumParameters(out int cantidadparm));
            StringBuilder dspname = new StringBuilder();
            //v2 errcheck(sourceDSP.getInfo(out string dspname, out uint dspversion,out int dspchannels,  out int dspeight, out int dspwidth));
            errcheck(sourceDSP.getInfo(dspname, out uint dspversion, out int dspchannels, out int dspeight, out int dspwidth));
            Console.WriteLine($"el plugin {dspname} versión{dspversion} tiene {cantidadparm} cantidad de parámetros. ");
            for (int i = 0; i < cantidadparm; i++)
            {
                errcheck(sourceDSP.getParameterInfo(i, out DSP_PARAMETER_DESC dspinfo));

                Console.WriteLine($"info del plugin { new string(dspinfo.name)} y {dspinfo.description }");

            }
            errcheck(jumpchannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, sourceDSP));
            

            VECTOR pos = new VECTOR { x = -10, y = -20, z = 0 };
            VECTOR relativePos = new VECTOR { x = pos.x - listenerPos.x, y = pos.y - listenerPos.y, z = pos.z - listenerPos.z };

            VECTOR vel = new VECTOR { x = 0, y = 0, z = 0 };
            VECTOR altpan = new VECTOR { x = 0, y = 0, z = 0 };
            DSP_PARAMETER_3DATTRIBUTES atr3d = new DSP_PARAMETER_3DATTRIBUTES();
            atr3d.absolute = new _3D_ATTRIBUTES{position= pos, velocity= vel, forward= listenerForward, up=listenerUp};
            atr3d.relative = new _3D_ATTRIBUTES{ position = relativePos, velocity = vel, forward = listenerForward, up = listenerUp };
            errcheck(sourceDSP.setParameterFloat(2, 1.0f)); //Min distance
            errcheck(sourceDSP.setParameterFloat(3, 100.0f)); //Max distance

            byte[] dspdatabytes = new byte[Marshal.SizeOf(typeof(DSP_PARAMETER_3DATTRIBUTES))];
            GCHandle pinStructure = GCHandle.Alloc(atr3d, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), dspdatabytes, 0, dspdatabytes.Length);
                Console.WriteLine($" largo de los bytes {dspdatabytes.Length}");
                errcheck(sourceDSP.setParameterData(8,dspdatabytes));


            }
            catch (Exception e)
            {
                Console.WriteLine($"Prolemas al copiar convertir los bytes {e.Message}");
                
            }
            finally
            {
                pinStructure.Free();

            }

            errcheck(jumpchannel.set3DAttributes(ref pos, ref vel, ref altpan));
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
                Console.WriteLine("bien ");
            }
        }
    }
}

