﻿using Microsoft.Xna.Framework;
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
        int cooldown = 0;
        SpriteBatch spriteBatch;
        FMOD.System fmod;
        ChannelGroup masterChannel;
        ChannelGroup world3DChannel;
        Channel jumpchannel;
        uint raHandle;
        uint ralistenerHandle;
        uint raSourceHandle;
        DSP listenerDSP;
        DSP sourceDSP;
        VECTOR listenerPos = new VECTOR { x = 0, y = 0, z = 0 };
        VECTOR listenerVel = new VECTOR { x = 0, y = 0, z = 0 };
        VECTOR listenerForward = new VECTOR { x = 0, y = 0, z = 1 };
      
        VECTOR listenerUp = new VECTOR { x = 0, y = 1, z = 0};
        int angulo = 0;

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
            //errcheck(fmod.loadPlugin("plugins/resonanceaudio.dll", out raHandle));
            errcheck(fmod.loadPlugin("plugins/OculusSpatializerFMOD.dll", out raHandle));
          

            errcheck(fmod.createDSPByPlugin(raHandle, out DSP raDSP));
            // StringBuilder radspinfo = new StringBuilder();

            errcheck(raDSP.getInfo(out string radspinfo, out uint radspversion, out int radspchannels, out int radspheight, out int radspwidth));
          
            Console.WriteLine($" detalles de dsp: {radspinfo} {radspversion} ");
            errcheck(fmod.getNestedPlugin(raHandle, 0, out ralistenerHandle));
            errcheck(fmod.getNestedPlugin(raHandle, 1, out uint rasoundfieldHandle));
            errcheck(fmod.getNestedPlugin(raHandle, 2, out raSourceHandle));
            errcheck(fmod.createDSPByPlugin(raSourceHandle, out listenerDSP));
            // StringBuilder listenerdspinfo = new StringBuilder();
            errcheck(listenerDSP.getInfo(out string listenerdspinfo, out uint listenerdspversion, out int listenerdspchannels, out int listenerdspheight, out int listenerdspwidth));
          
            Console.WriteLine($" detalles de resonanse audio listener dsp: {listenerdspinfo} {listenerdspversion} ");
            errcheck(fmod.createDSPByPlugin(rasoundfieldHandle, out DSP soundfieldDSP));
            // StringBuilder soundfielddspinfo = new StringBuilder();
            errcheck(soundfieldDSP.getInfo(out string soundfielddspinfo, out uint soundfielddspversion, out int soundfielddspchannels, out int soundfielddspheight, out int soundfielddspwidth));
          
            Console.WriteLine($" detalles de resonanse audio soundfield dsp: {soundfielddspinfo} {soundfielddspversion} ");
            errcheck(fmod.createDSPByPlugin(raSourceHandle, out DSP sourceDSP));
            // StringBuilder sourcedspinfo = new StringBuilder();

            errcheck(sourceDSP.getInfo(out string sourcedspinfo, out uint sourcedspversion, out int sourcedspchannels, out int sourcedspheight, out int sourcedspwidth));
          
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
          
          
            errcheck(fmod.playSound(jumpsound, masterChannel, paused: true, channel: out jumpchannel));
          
            //errcheck(jumpchannel.setMode(MODE._3D));
            errcheck(jumpchannel.setChannelGroup(masterChannel));
            errcheck(fmod.createDSPByPlugin(raHandle, out sourceDSP));
            errcheck(sourceDSP.getNumParameters(out int cantidadparm));
            // StringBuilder dspname = new StringBuilder();
            errcheck(sourceDSP.getInfo(out string dspname, out uint dspversion,out int dspchannels,  out int dspeight, out int dspwidth));
          
            Console.WriteLine($"el plugin {dspname} versión{dspversion} tiene {cantidadparm} cantidad de parámetros. ");
            for (int i = 0; i < cantidadparm; i++)
            {
                errcheck(sourceDSP.getParameterInfo(i, out DSP_PARAMETER_DESC dspinfo));

                Console.WriteLine($"info del plugin { new string(dspinfo.name)} y {dspinfo.description }");

            }
            errcheck(jumpchannel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, sourceDSP));
            errcheck(listenerDSP.setParameterBool(0, false)); // activa el reflection
            errcheck(listenerDSP.setParameterBool(1, true)); // activa el reberb
          

            setSourceParameters();

            //errcheck(jumpchannel.set3DAttributes(ref pos, ref vel, ref altpan));
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

        void setSourceParameters()
        {
            
            
            VECTOR pos = new VECTOR { x = 1.5f, y = 0, z = 0 };
            //VECTOR relativePos = new VECTOR { x = pos.x - listenerPos.x, y = pos.y - listenerPos.y, z = pos.z - listenerPos.z };
             VECTOR relativePos = new VECTOR { x = 0, y= 0, z=0};

            VECTOR vel = new VECTOR { x = 0, y = 0, z = 0 };
            VECTOR altpan = new VECTOR { x = 0, y = 0, z = 0 };
            DSP_PARAMETER_3DATTRIBUTES atr3d = new DSP_PARAMETER_3DATTRIBUTES();

            atr3d.absolute = new ATTRIBUTES_3D { position = pos, velocity = vel, forward = listenerForward, up = listenerUp};
            atr3d.relative = new ATTRIBUTES_3D { position = relativePos, velocity = vel, forward = altpan, up = altpan };
            errcheck(sourceDSP.setParameterBool(1, true)); // reflections
            errcheck(sourceDSP.setParameterBool(2, true)); // internal atenuation
            errcheck(sourceDSP.setParameterFloat(3, 0.2f)); //volumetric radiyus
            errcheck(sourceDSP.setParameterFloat(4, 0.2f)); //Min distance
            errcheck(sourceDSP.setParameterFloat(5, 20f)); //Max distance
          
            byte[] dspdatabytes = new byte[Marshal.SizeOf(typeof(DSP_PARAMETER_3DATTRIBUTES))];
            GCHandle pinStructure = GCHandle.Alloc(atr3d, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), dspdatabytes, 0, dspdatabytes.Length);
                Console.WriteLine($" largo de los bytes {dspdatabytes.Length}");
                errcheck(sourceDSP.setParameterData(0, dspdatabytes));


            }
            catch (Exception e)
            {
                Console.WriteLine($"Prolemas al copiar convertir los bytes {e.Message}");

            }
            finally
            {
                pinStructure.Free();

            }
            
            
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
          
            if(cooldown>0)
            {
                cooldown--;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState state = Keyboard.GetState();
        foreach(Keys k in state.GetPressedKeys())
            {
                if(cooldown>0)
                {
                    break;
                }
                switch(k)
                {
                    case Keys.D:
                        listenerPos.x += 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.A:
                        listenerPos.x -= 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.W:
                        listenerPos.z += 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.S:
                        listenerPos.z -= 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.R:
                        listenerPos.y += 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.F:
                        listenerPos.y -= 0.1f;
                        cooldown = 30;
                        break;
                    case Keys.X:
                        angulo += 10;
                        cooldown = 15;
                        if(angulo>360)
                        {
                            angulo = 0;
                        }
                        break;
                    case Keys.Z:
                        angulo -= 10;
                        cooldown = 20;
                        if(angulo<0)
                        {
                            angulo = 360;
                        }
                        break;
                    case Keys.P:
                        
                        jumpchannel.getPaused(out bool paused);
                        jumpchannel.setPaused(!paused);
                        break;

                }
            }

            listenerForward.x = (float)Math.Sin((angulo * Math.PI) / 180);
            listenerForward.z = (float)Math.Cos((angulo * Math.PI) / 180);
            errcheck(fmod.set3DListenerAttributes(0, ref listenerPos, ref listenerVel, ref listenerForward, ref listenerUp));
            setSourceParameters();
            errcheck(fmod.update());
            base.Update(gameTime);
            
            Console.WriteLine($" POSICIÓN ACTUAL DEL LISTENER: {listenerPos.x}, {listenerPos.y} {listenerPos.z}. Con su ángulo {angulo}");
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

