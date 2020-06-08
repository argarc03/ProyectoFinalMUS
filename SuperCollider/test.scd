s.boot



/**********************************************************/
//                     BUFFER DICTIONARY                 //
/********************************************************/

(
d = Dictionary.new;
d.add(\drums -> PathName("Audio/Drums/").entries.collect({
	arg sf;
	Buffer.read(s, sf.fullPath);
    })
);

d.add(\piano -> PathName("Audio/Piano/").entries.collect({
	arg sf;
	Buffer.read(s, sf.fullPath);
    })
)
)





/**********************************************************/
//                     SYNTH DEFINITIONS                 //
/********************************************************/

// Sin Synth

(
var crash = d[\drums][0],
fTom = d[\drums][1],
hTom = d[\drums][2],
hhat = d[\drums][3],
kick = d[\drums][4],
mTom = d[\drums][5],
ride = d[\drums][6],
snare = d[\drums][7],
do = 261.63,
re = 293.66,
mi = 329.63,
fa = 349.23,
sol = 392,
la = 440,
si = 493.88,
doA = 523.25,
dor = 1.059463**(-9),
rer = 1.059463**(-7),
mir = 1.059463**(-5),
far = 1.059463**(-4),
solr = 1.059463**(-2),
lar = 1.059463**(0),
sir = 1.059463**(2),
doAr = 1.059463**(3);


SynthDef(\sinSynth,
	{
		arg out = 0,
		freq = 440,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal = [SinOsc.ar(freq, 0, 0.2), SinOsc.ar(freq, 0, 0.2)];

		Out.ar(out, signal*amp);
}).add;


// Square Synth


SynthDef(\squareSynth,
	{
		arg out = 0,
		freq = 440,
		amp = 0.2,
		buffer = 0;
		var signal;

		signal = [LFPulse.ar(freq, 0, 0.2), LFPulse.ar(freq, 0, 0.2)];

		Out.ar(out, signal*amp);
}).add;


/**********************************************************************/
// FLUTE
// Originally found at http://ecmc.rochester.edu/ecmc/docs/supercollider/scbook/Ch21_Interface_Investigations/ixi%20SC%20tutorial/ixi_SC_tutorial_10.html

SynthDef.new(\fluteSynth, { arg scl = 0.2, freq = 440, ipress = 0.9, ibreath = 0.09, ifeedbk1 = 0.4, ifeedbk2 = 0.4, dur = 1, gate = 1, amp = 2;

	var kenv1, kenv2, kenvibr, kvibr, sr, cr, block;
	var poly, signalOut, ifqc;
	var aflow1, asum1, asum2, afqc, atemp1, ax, apoly, asum3, avalue, atemp2, aflute1;
	var fdbckArray;

	sr = SampleRate.ir;
	cr = ControlRate.ir;
	block = cr.reciprocal;

	ifqc = freq;

	// noise envelope
	kenv1 = EnvGen.kr(Env.new(
		[ 0.0, 1.1 * ipress, ipress, ipress, 0.0 ], [ 0.06, 0.2, dur - 0.46, 0.2 ], 'linear' )
	);
	// overall envelope
	kenv2 = EnvGen.kr(Env.new(
		[ 0.0, amp, amp, 0.0 ], [ 0.1, dur - 0.02, 0.1 ], 'linear' ), doneAction: 2
	);
	// vibrato envelope
	kenvibr = EnvGen.kr(Env.new( [ 0.0, 0.0, 1, 1, 0.0 ], [ 0.5, 0.5, dur - 1.5, 0.5 ], 'linear') );

	// create air flow and vibrato
	aflow1 = LFClipNoise.ar( sr, kenv1 );
	kvibr = SinOsc.ar( 5, 0, 0.1 * kenvibr );

	asum1 = ( ibreath * aflow1 ) + kenv1 + kvibr;
	afqc = ifqc.reciprocal - ( asum1/20000 ) - ( 9/sr ) + ( ifqc/12000000 ) - block;

	fdbckArray = LocalIn.ar( 1 );

	aflute1 = fdbckArray;
	asum2 = asum1 + ( aflute1 * ifeedbk1 );

	//ax = DelayL.ar( asum2, ifqc.reciprocal * 0.5, afqc * 0.5 );
	ax = DelayC.ar( asum2, ifqc.reciprocal - block * 0.5, afqc * 0.5 - ( asum1/ifqc/cr ) + 0.001 );

	apoly = ax - ( ax.cubed );
	asum3 = apoly + ( aflute1 * ifeedbk2 );
	avalue = LPF.ar( asum3, 2000 );

	aflute1 = DelayC.ar( avalue, ifqc.reciprocal - block, afqc );

	fdbckArray = [ aflute1 ];

	LocalOut.ar( fdbckArray );

	signalOut = avalue;

	OffsetOut.ar( 0, [ signalOut * kenv2, signalOut * kenv2 ] );

}).add;

/**********************************************************************/

/**********************************************************************/
// GUITAR

SynthDef.new(\guitarSynth, {arg amp = 0.3, freq = 440, decay = 5, coef = 0.1;
	var env, snd;
	env = EnvGen.kr(Env.linen(0, decay, 0), doneAction: 2);
	snd = Pluck.ar(
		in: WhiteNoise.ar(amp),
		trig: Impulse.kr(0),
		maxdelaytime: 0.1,
		delaytime: freq.reciprocal,
		decaytime: decay,
		coef: coef);
	Out.ar(0, [snd, snd]);
}).add;

/**********************************************************************/

/**********************************************************************/
// VIOLIN

/* Retrieved from
http://sccode.org/1-5aD
by nicolaariutti
http://sccode.org/1-5as
Modifications from the original include:
-Added Vibrato.ar, to better simulate left hand movement alongside the pre-existing pwm.
-Changed ASR envelope to ADSR and "scratch" envelopes, to simulate the attack of a bow.
-Added filters, to simulate tone brightening from the bridge and back of the instrument.
Modified by Bruno Ruviaro and Josh Mitchell 8/19.
*/

SynthDef(\violinSynth, {
	arg
	//Standard Definitions
	freq = 440, gate = 1, amp = 1, pan = 0, out = 0, att = 0.1, dec = 0.1, sus = 0.5, rel = 0.1,
	//Vibrato Controls
	vRate = 4.6, vDepth = 0.02, vAtt = 0.15, vRateVar = 0.25, vDepthVar = 0.05,
	//PWM Controls (pwmMax and pwmMin are 0 - 1)
	pwmVarRate = 2, pwmMin = 0.7, pwmMax = 0.8, pwmRate = 5,
	//Other Controls
	bridgeFreq = 2500, scratchDepth = 0.15;

	var scratch, env, pwm, snd;

	scratch = 1.015 + Env.perc(att, dec * 1.25, scratchDepth).kr;

	env = Env.adsr(att, dec, sus, rel).kr(gate: gate, doneAction: 2);

	freq = Vibrato.kr(
		freq: freq,
		rate: vRate,
		depth: vDepth,
		delay: (att + dec),
		onset: vAtt,
		rateVariation: vRateVar,
		depthVariation: vDepthVar
	);

	pwm = SinOsc.kr(freq: pwmRate, phase: Rand(0.0, 1.0)).range(pwmMin, pwmMax);
	pwm = pwm * LFNoise2.kr(pwmVarRate).range(0.2, 0.8);

	snd = VarSaw.ar(
		freq: Lag.kr(freq) * LFPulse.ar(freq * 1.5).range(1/scratch, scratch),
		width: pwm,
		mul: amp
	);

	snd = (snd * 0.7) + BPF.ar(snd, bridgeFreq, 2, 2);
	snd = snd + HPF.ar(snd, bridgeFreq * 2);
	snd = snd * env;
	Out.ar(out, Pan2.ar(snd, pan));
},
metadata: (
	credit: "Original by nicolaariutti, modified by Josh Mitchell",
	category: \strings,
	tags: [\bowed, \pitched, \violin]
)
).add;

/**********************************************************************/

/**********************************************************************/
// BELLS

/*
Retrieved from
http://sccode.org/1-5aD
Tibetan prayer bells acoustically modeled
by wondersluyter
http://sccode.org/wondersluyter
Slightly modified by Bruno Ruviaro, 2019.
*/

SynthDef.new(\bellSynth, { arg out, singSwitch = 0, freq = 2434, amp = 0.5, decayScale = 1, lag = 10, pan = 0;
	var snd, input, first, freqScale, mallet, sing;
	freqScale = freq / 2434;
	freqScale = Lag3.kr(freqScale, lag);
	decayScale = Lag3.kr(decayScale, lag);

	// mallet
	mallet = LPF.ar(
		in: Impulse.ar(0) ! 2 * amp,
		freq: 10000 * freqScale
	);

	// sing
	sing = LPF.ar(
		in: {PinkNoise.ar * Integrator.kr(singSwitch * 0.001, 0.999).linexp(0, 1, 0.01, 1) * amp} ! 2,
		freq: 2434 * freqScale
	);

	sing = sing + Dust.ar(0.1);
	sing = LPF.ar(sing, 10000 * freqScale);
	sing = sing * LFNoise1.kr(0.5).range(-45, -30).dbamp;

	// input = mallet + sing
	input = mallet + (singSwitch.clip(0, 1) * sing);

	// resonant filter bank
	snd = DynKlank.ar(
		specificationsArrayRef: `[
			// Array of filter frequencies
			[
				(first = LFNoise1.kr(0.5).range(2424, 2444)) + Line.kr(20, 0, 0.5),
				first + LFNoise1.kr(0.5).range(1,3),
				LFNoise1.kr(1.5).range(5435, 5440) - Line.kr(35, 0, 1),
				LFNoise1.kr(1.5).range(5480, 5485) - Line.kr(10, 0, 0.5),
				LFNoise1.kr(2).range(8435, 8445) + Line.kr(15, 0, 0.05),
				LFNoise1.kr(2).range(8665, 8670),
				LFNoise1.kr(2).range(8704, 8709),
				LFNoise1.kr(2).range(8807, 8817),
				LFNoise1.kr(2).range(9570, 9607),
				LFNoise1.kr(2).range(10567, 10572) - Line.kr(20, 0, 0.05),
				LFNoise1.kr(2).range(10627, 10636) + Line.kr(35, 0, 0.05),
				LFNoise1.kr(2).range(14689, 14697) - Line.kr(10, 0, 0.05)
			],
			// Array of filter amplitudes
			[
				LFNoise1.kr(1).range(-10, -5).dbamp,
				LFNoise1.kr(1).range(-20, -10).dbamp,
				LFNoise1.kr(1).range(-12, -6).dbamp,
				LFNoise1.kr(1).range(-12, -6).dbamp,
				-20.dbamp,
				-20.dbamp,
				-20.dbamp,
				-25.dbamp,
				-10.dbamp,
				-20.dbamp,
				-20.dbamp,
				-25.dbamp
			],
			// Array of filter decay times
			[
				20 * freqScale.pow(0.2),
				20 * freqScale.pow(0.2),
				5,
				5,
				0.6,
				0.5,
				0.3,
				0.25,
				0.4,
				0.5,
				0.4,
				0.6
			] * freqScale.reciprocal.pow(0.5)
		],
		input: input,
		freqscale: freqScale,
		freqoffset: 0,
		decayscale: decayScale
	);

	DetectSilence.ar(snd, doneAction: 2);

	Out.ar(out, Pan2.ar(Mix.ar(snd), pan));
},
metadata: (
	credit: "by wondersluyter",
	category: \bells,
	tags: [\percussion, \bell, \prayer, \tibetan]
)
).add;

/**********************************************************************/

// Drums

SynthDef.new(\drumBuf, {
	arg buf = 0, rate = 1, amp = 1;
	var sig;
	sig = PlayBuf.ar(2, buf, BufRateScale.ir(buf) * rate, doneAction:2); // DoneAction 2 = free the synth when done
	sig = sig*amp;
	Out.ar(0, sig);
}).add;


// Piano Buffer

SynthDef.new(\pianoBuf, {
	arg buf = 0, rate = 1, amp = 1;
	var sig;
	sig = PlayBuf.ar(2, buf, BufRateScale.ir(buf) * rate, doneAction:2); // DoneAction 2 = free the synth when done
	sig = sig*amp;
	Out.ar(0, sig);
}).add;

/******************************************/
//                 PRESETS                //
/******************************************/


// Bell Preset
Pdef.new(
	\bellPdef,
	Pbind(
	    \instrument, \bellSynth,
		\dur, Pseq([1],inf),//Pseq([1/160], inf),
		\scale, Scale.major,
		\degree, Pseq([1,3,2,3,4,2,4,1],inf),
		\octave, 7,
	    //\delta, Pseq([1.20], inf),
	);
);


// Rhythm Presets
//1
Pdef.new(
	\drumPdef1,
	Pbind(
	    \instrument, \drumBuf,
	    \dur, Pseq([1/16], inf),
		\stretch, 60/120 * 4,
	    \buf, Pseq([kick,hhat,hhat,snare,hhat,kick,ride,ride,ride],inf),
	    \delta, Pseq([0.20], inf),
	    \rate, 1,
	    \amp, 0.35,
	);
);

//2
Pdef.new(
	\drumPdef2,
	Pbind(
	    \instrument, \drumBuf,
	    \dur, Pseq([1,1,1/2,1/4,1/4,1], inf),
		//\stretch, 60/210 * 4,
		\buf, Pseq([kick, hTom, fTom,mTom,mTom, [ride,mTom]],inf),
	    //\delta, Pseq([0.20], inf),
	    \rate, 1,
	    \amp, 0.35,
	);
);


// Flute Preset

Pdef.new(
	\flutePdef,
	Pbind(
	    \instrument, \fluteSynth,
	    \dur, 1,//Pseq([1/16], inf),
		//\stretch, 60/120 * 4,
		\freq, Pseq([[do, mi, sol],mi,fa,do,[fa, la, doA],mi,fa, doA],inf),
	    //\delta, Pseq([0.70], inf),
	    //\rate, 1,
	    \amp, 0.35,
	);
);


// Guitar Preset

Pdef.new(
	\guitarPdef,
	Pbind(
	    \instrument, \guitarSynth,
	    \dur, Pseq([1,1,0.5,0.5,1], inf),
		//\stretch, 60/120 * 4,
		\freq, Pseq([[fa, la, doA],[mi,sol,si],mi,fa,[do, mi, sol, si]],inf),
	    //\delta, Pseq([0.70], inf),
	    //\rate, 1,
	    \amp, 0.35,
	);
);


// Piano Preset

Pdef.new(
	\pianoPdef,
	Pbind(
	    \instrument, \pianoBuf,
		\buf, d[\piano][0],
		\dur, Pseq([1,1,1,1/2,1/2,1,1,1,1/2,1/2],inf),
		\rate, Pseq([[dor,mir,solr],mir,solr,sir,sir,[rer, far, lar], far, lar,sir,lar],inf),
	    \amp, 0.35,
	);
);


// Violin Preset

Pdef.new(
	\violinPdef,
	Pbind(
	    \instrument, \violinSynth,
		\dur, Pseq([1,1/2,1/2,1,1/2,1/2], inf),
		\freq, Pseq([doA, sol, fa, si,  fa, sol],inf),
		//\delta, Pseq([0.70], inf),
	    //\rate, 1,
	    \amp, 0.35,
	);
)
)

/**********************************************************/
//                     END OF SYNTH DEFINITIONS          //
/********************************************************/


Pdef(\bellPdef).play;

Pdef(\bellPdef).stop;

/**********************************************************/
//                     OSC DEFINITIONS                   //
/********************************************************/
(
	// Variables
	var square, sin = nil, flute, guitar, violin, bell, drum, pianoBuf, objects;

	// Stop all

OSCdef.new(
	\stopAll,
	{arg msg;
		[msg].postln;

		Server.freeAll();

		// Stop every object

		objects.do({
			arg item, i;
			item.stop;
			item.free();
			item = nil;
		});
	},
	'/stopAll',
	nil, 57120
);



	// Sin Preview

	OSCdef.new(
		\sin,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				sin.free();
				sin = nil;
			}
			{
				sin = Synth(\sinSynth, [\freq, [msg][0][2]])
			}
		},
		'/sin',
		nil, 57120
	);


	// Square Preview

	square = nil;

	OSCdef.new(
		\square,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				square.free();
				square = nil;
			}
			{
				square = Synth(\squareSynth, [\freq, [msg][0][2]])
			}
		},
		'/square',
		nil, 57120
	);


	// Flute Preview

	flute = nil;

	OSCdef.new(
		\flute,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				flute.free();
				flute = nil;
			}
			{
				flute = Synth(\fluteSynth, [\freq, [msg][0][2]])
			}
		},
		'/flute',
		nil, 57120
	);


	// Guitar Preview

	guitar = nil;

	OSCdef.new(
		\guitar,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				guitar.free();
				guitar = nil;
			}
			{
				guitar = Synth(\guitarSynth, [\freq, [msg][0][2]])
			}
		},
		'/guitar',
		nil, 57120
	);


	// Violin Preview

	violin = nil;

	OSCdef.new(
		\violin,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				violin.free();
				violin = nil;
			}
			{
				violin = Synth(\violinSynth, [\freq, [msg][0][2]])
			}
		},
		'/violin',
		nil, 57120
	);


	// Bell Preview

	bell = nil;

	OSCdef.new(
		\bell,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				bell.free();
				bell = nil;
			}
			{
				bell = Synth(\bellSynth, [\freq, [msg][0][2]])
			}
		},
		'/bell',
		nil, 57120
	);


	// Drum Preview

	drum = nil;
	OSCdef.new(
		\drums,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				drum.free();
				drum = nil;
			}
			{
				drum = Synth(\drumBuf, [\buf, d[\drums][[msg][0][2]]])
			}
		},
		'/drums',
		nil, 57120
	);


	// Piano Buff Preview

	pianoBuf = nil;
	OSCdef.new(
		\piano,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
				pianoBuf.free();
				pianoBuf = nil;
			}
			{
				pianoBuf = Synth(\pianoBuf, [\buf, d[\piano][0],
					\rate, 1.059463**([msg][0][2])])
			}
		},
		'/piano',
		nil, 57120
	);


	// **** SOUND OBJECTS **** //

	objects = Array.newClear(indexedSize: 50);

	// Sin

	OSCdef.new(
		\sinLoop,
		{arg msg;
			[msg].postln;
			if ( [msg][0][1] == -1.0)
			{
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Synth(\sinLoopSynth)
			}
		},
		'/sinLoop',
		nil, 57120
	);

	// Square

	OSCdef.new(
		\squareLoop,
		{arg msg;
			[msg].postln;

			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Synth(\squareLoopSynth)
			}
		},
		'/squareLoop',
		nil, 57120
	);



	// Move sound to new index

	OSCdef.new(
		\move,
		{arg msg;
			[msg].postln;
			objects[[msg][0][2]] = objects[[msg][0][1]];

		},
		'/move',
		nil, 57120
	);



	/******************************************************/
	// PRESETS OSC
    /******************************************************/

	// Rhythm 1 preset
	OSCdef.new(
		\drumPreset1,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\drumPdef1).play;
			}
		},
		'/drumPreset1',
		nil, 57120
	);

    // Rhythm 2 preset
	OSCdef.new(
		\drumPreset2,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\drumPdef2).play;
			}
		},
		'/drumPreset2',
		nil, 57120
	);

    // BELLS preset
    OSCdef.new(
		\bellPreset,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\bellPdef).play;
			}
		},
		'/bellPreset',
		nil, 57120
	);

    // FLUTE preset
    OSCdef.new(
		\flutePreset,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\flutePdef).play;
			}
		},
		'/flutePreset',
		nil, 57120
	);

    // GUITAR preset
    OSCdef.new(
		\guitarPreset,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\guitarPdef).play;
			}
		},
		'/guitarPreset',
		nil, 57120
	);

    // PIANO preset
    OSCdef.new(
		\pianoPreset,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\pianoPdef).play;
			}
		},
		'/pianoPreset',
		nil, 57120
	);

    // VIOLIN preset
    OSCdef.new(
		\violinPreset,
		{arg msg;
			[msg].postln;


			if ( [msg][0][1] == -1.0)
			{
			    objects[[msg][0][2]].stop;
				objects[[msg][0][2]].free();
				objects[[msg][0][2]] = nil;
			}
			{
				objects[[msg][0][2]] = Pdef(\violinPdef).play;
			}
		},
		'/violinPreset',
		nil, 57120
	);
)
/**********************************************************/
//                     END OF OSC DEFINITIONS            //
/********************************************************/

ScIDE.handshake; // Ejecutar si da error al guardar




// LOOPS

// Sin LOOP
(
SynthDef(\sinLoopSynth,
	{
		arg out = 0,
		rate = 1,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal = Pluck.ar([SinOsc.ar(440, 0, 0.2), SinOsc.ar(440, 0, 0.2)], Impulse.kr(2));

		Out.ar(out, signal*amp);
}).add;
)

// Square LOOP
(
SynthDef(\squareLoopSynth,
	{
		arg out = 0,
		rate = 1,
		amp = 0.1,
		buffer = 0;
		var signal;

		signal = Pluck.ar([LFPulse.ar(440, 0, 0.2), LFPulse.ar(440, 0, 0.2)], Impulse.kr(2));

		Out.ar(out, signal*amp);
}).add;
)




// TEST

play{SinOsc.ar(OnePole.ar(Mix(
	LFSaw.ar([1,0.99],[0,0.6],2000,2000).trunc([400,600])*[1,-1]
),0.98)).dup*0.1}

//// PIANO TEST ////
(
Pdef(
	\pianoRhythm,
	Pbind(
		\instrument, \pianoBuf,
		\delta, Prand([0.5],inf),
		\buf, d[\piano][0],
		\rate, Pseq([1*(1.059463**(-9)),
			1*(1.059463**(-8)),
			//1*(1.059463**(-7)),
			1*(1.059463**(-6)),
			//1*(1.059463**(-5)),
			1*(1.059463**(-4)),
			1*(1.059463**(-3)),
			//1*(1.059463**(-2)),
			1*(1.059463**(-1)),
			//1*(1.059463**(0)),
			1*(1.059463**(1)),
			1*(1.059463**(2)),
			//1*(1.059463**(3))
		],1),
		\amp, 0.5,
	);
).play;
)




(
Pdef.new(
	\test,
	Pbind(
	    \instrument, \sinSynth,
		\dur, Pseq([2.2,2.2,2.05,2.2,2.2,2.2,2.05,2.2],inf),//Pseq([1/160], inf),
		\scale, Scale.minor,
		\degree, Pseq([3,1,2,3,3,3,2,1],inf),
		\octave, 6,
	    //\delta, Pseq([1.20], inf),
	);
)
)
var test = Pdef(\test).play;

test.free();
test = nil;


p.stop;


// R1
(
var crash = d[\drums][0],
fTom = d[\drums][1],
hTom = d[\drums][2],
hhat = d[\drums][3],
kick = d[\drums][4],
mTom = d[\drums][5],
ride = d[\drums][6],
snare = d[\drums][7];
Pdef.new(
	\test,
	Pbind(
	    \instrument, \drumBuf,
	    \dur, Pseq([1,1,1/2,1/4,1/4,1], inf),
		//\stretch, 60/210 * 4,
		\buf, Pseq([kick, hTom, fTom,mTom,mTom, [ride,mTom]],inf),
	    //\delta, Pseq([0.20], inf),
	    \rate, 1,
	    \amp, 0.35,
	);
)
)

Pdef(\test).play;

Pdef(\test).stop;


(
var do = 261.63,
re = 293.66,
mi = 329.63,
fa = 349.23,
sol = 392,
la = 440,
si = 493.88,
doA = 523.25;
Pdef.new(
	\test,
	Pbind(
	    \instrument, \violinSynth,
		\dur, Pseq([1,1/2,1/2,1,1/2,1/2], inf),
		\freq, Pseq([doA, sol, fa, si,  fa, sol],inf),
		//\delta, Pseq([0.70], inf),
	    //\rate, 1,
	    \amp, 0.35,
	);
)
)

Pdef(\test).play;

Pdef(\test).stop;









////////////////////////////