import numpy as np
import scipy as sci
class OrbitOptimizer:
	def __init__(self):
		self.G = 6.67430e-11  # Gravitational constant
		self.EARTH_MASS = 5.97e24  # Mass of Earth
		self.EARTH_RADIUS = 6371000  # Meters
		self.MAX_THRUST = 150.0 #Newtons
		self.ISP = 1500.0 #seconds
		self.ROCKET_STAGES = [[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0]]
		self.ROCKET_MASS = 556000

	# F = m*a = m*zddot
	# z == altitude from center of Earth North
	# x ==  altitude from center west / east
	# zdot = velocity
	# zddot = acceleration
	## Second Order Differential Equation
	def Derivatives(self, state, t):
		# state vector
		x = state[0]
		z = state[1]
		velx = state[2]
		velz = state[3]
		mass = state[4]
		aeroX = state[5]
		aeroY = state[6]
		#Compute zdot
		zdot = velz
		xdot = velx

		#print("Altitude: ", x**2 + z**2)
		if np.sqrt(x**2 + z**2) < self.EARTH_RADIUS:
			xdot = 0
			zdot = 0
			ddot = np.asarray([0.0, 0.0])
			return np.asarray([xdot, zdot, ddot[0], ddot[1], 0.0, 0.0, 0.0])
			
		# Compute Forces
		## Gravity Array
		gravityForce  = -self.Gravity(z, x) * mass
		## AeroDynamics
		aeroForce = -self.drag(t, velx, velz, x, z)
		
		## determine force direction based off velocity of the aeroCraft
		if(velx < 0):
			aeroForce[0] = -aeroForce[0]
		if(velz < 0):
			aeroForce[1] = -aeroForce[1]
		
		if abs(aeroForce[0]) + abs(aeroForce[1])  > 0:
			aeroForceDot = np.asarray([abs(aeroX) - abs(aeroForce[0]), abs(aeroY) - abs(aeroForce[1])])
		else:
			aeroForceDot = np.asarray([0.0, 0.0])  
		## Thrust
		## if mass is near 0 remove thrust force
		if mass > 0.5:
			thrustForce, mdot = self.propulsion(t)
		else:
			thrustForce = 0.0
			mdot = 0.0

		## Total Force 
		Forces = gravityForce + thrustForce + aeroForce
		#print("time: ", t, " Gravity: ", gravityForce, " Aero: ", aeroForce, " Thrust: ", thrustForce, " Forces: ", Forces)
		#print("Velocity: ", velx, " : ", velz)
		#print("mass:",mass,"time:", t)
		#compute acceleration Array
		
		ddot = Forces / mass
		#print("Timer: ", t)
		# compute the statedot
		statedot = np.asarray([xdot, zdot, ddot[0], ddot[1], mdot, aeroForceDot[0], aeroForceDot[1]])

		return statedot
  
	def ReadTxtFile(self,fileName):
		result = []
		Columns = []
		# Read the file
		file = open(fileName, "r", encoding='utf-8-sig')
		for line in file:
			result.append(line.strip().split(" "))
		# Convert to numpy array
		#print(result)
		result = np.array(result)

		for i in range(0, len(result[0])):
			Columns.append(result[1:,i])
		file.close()
		columns = np.array(Columns, dtype=float)
		return columns

	def  propulsion(self,t):
		# Default Values
		thrustF = 0.0
		mdot = 0.0
		theta = 0.0

		for i in range(0, len(self.ROCKET_STAGES)):
			stage = self.ROCKET_STAGES[i]
			#print(stage)
			stage_start = stage[0]
			stage_end = stage[1]
			stage_seperation_time = stage[2]
			stage_mass = stage[3]
			stage_thrust = stage[4]
			stage_thrust_angle = stage[5]
			stage_ISP = stage[6]
			#print("Time ", t)
			# Active Burn Phase
			if t > stage_start and t < stage_end:
				theta = stage_thrust_angle * np.pi/180
				thrustF = stage_thrust
				ve = stage_ISP*9.81
				# Mass loss from fuel usage
				mdot = -thrustF/ve
				#print("thrust: ",thrustF)
				#print("stage:", i, " mdot:", mdot)
				#print("stageStart:", stage_start)
				#print("Time ", t)
				break
			# Seperation Phase
			if t > stage_end and t < (stage_end + stage_seperation_time):
				thrustF = 0.0
				theta = 0
				# Mass loss from booster seperation
				mdot = -stage_mass/stage_seperation_time
				#print("mdot Rocket: ", mdot)
				break
			
			
		# Angle of thruster
		thrustX = thrustF * np.sin(theta)
		thrustY = thrustF * np.cos(theta)

		return np.asarray([thrustX, thrustY]),mdot


	def drag(self, t, velX, velY, x, z):
		altitude = np.sqrt(x**2 + z**2) - self.EARTH_RADIUS
		if (altitude > 100000):
			return np.asarray([0.0, 0.0])
		atmosModel = self.ReadTxtFile("StandardAtmosphereModel.txt")
		altitudeArray = atmosModel[0]
		densityArray = atmosModel[3]
		#print(densityArray)
		# Interpolate the density based on the altitude
		p = np.interp(altitude, altitudeArray, densityArray)
		#print("Altitude: ", altitude, " Density: ", p, "speed: ", velX, velY)
		Cd = 0.4 

		
		crossSection = np.pi * (0.3/2)**2
		# cross section of Falcon 9
		crossSection = np.pi * (3.7/2)**2
		DragFX = 1/2 * Cd * p * velX**2 * crossSection
		DragFY = 1/2 * Cd * p * velY**2 * crossSection 
		# Cd drag Coefficient
		# p = air density
		# A = cross section
		return  np.asarray([DragFX, DragFY])

	def Gravity(self, z, x):
		radius = np.sqrt(x**2 + z**2)

		if radius < self.EARTH_RADIUS:
			accelX = 0.0
			accelZ = 0.0
		else:
			accelX = self.G * self.EARTH_MASS /(radius**3) * x
			accelZ = self.G * self.EARTH_MASS /(radius**3) * z
		#print("Gravity: ", accelX, " : ", accelZ)
		return np.asarray([accelX, accelZ])

	def DefineRocketStages(self, Num, StartTime, EndTime, SeperationTime, StageMass, StageThrust, ThrustAngle, ISP, rocketMass):
		for x in range(0, Num):
			stageProperties = []
			stageProperties.append(StartTime[x])
			stageProperties.append(EndTime[x])
			stageProperties.append(SeperationTime[x])
			stageProperties.append(StageMass[x])
			stageProperties.append(StageThrust[x])
			stageProperties.append(ThrustAngle[x])
			stageProperties.append(ISP[x])
			self.ROCKET_STAGES[x] = stageProperties
			self.ROCKET_MASS = rocketMass
		return self.ROCKET_STAGES
	
	def TrajectoryCalculation(self, timeWindow, x0, z0, velx0, velz0):

		stateInitial = np.asarray([x0, z0 + self.EARTH_RADIUS, velx0, velz0, self.ROCKET_MASS, 0.0, 0.0])
		# time Window
		if timeWindow < 100:
			tOut = np.linspace(0,timeWindow, timeWindow*100)
		if timeWindow < 250:
			tOut = np.linspace(0,timeWindow, timeWindow*25)
		if timeWindow < 500:
			tOut = np.linspace(0,timeWindow, timeWindow*10)
		if timeWindow < 5000:
			tOut = np.linspace(0,timeWindow, timeWindow)
		if timeWindow >= 5000 and timeWindow < 10000:
			tOut = np.linspace(0,timeWindow, int(timeWindow/10))
		if timeWindow >= 10000:
			tOut = np.linspace(0,timeWindow, int(timeWindow/25))
		
		stateout = sci.integrate.odeint(self.Derivatives, stateInitial, tOut)
		#stateout = sci.integrate.solve_ivp(self.Derivatives, tOut, stateInitial)
		xArray = stateout[:,0]
		yArray = stateout[:,1]
		xVelArray = stateout[:,2]
		yVelArray = stateout[:,3]
		satelliteMass = stateout[:,4]
		xAeroForce = stateout[:,5]
		yAeroForce = stateout[:,6]
		AeroForce = np.sqrt(xAeroForce**2 + yAeroForce**2)
		altitude = np.sqrt(xArray**2 + yArray**2) - self.EARTH_RADIUS
		normalisedVelocity = np.sqrt(xVelArray**2 + yVelArray**2)

		return {
		"xArray": xArray,
		"yArray": yArray,
		"xVelArray": xVelArray,
		"yVelArray": yVelArray,
		"altitude": altitude,
		"normalisedVelocity": normalisedVelocity,
		"satelliteMass": satelliteMass,
		"xAeroForce": xAeroForce,
		"yAeroForce": yAeroForce,
		"AeroForce": AeroForce,
		"timeFrame": tOut
		}


	def PlotPlanet(self):
		theta = np.linspace(0,2*np.pi,1000)
		xPlanet = self.EARTH_RADIUS*np.cos(theta)
		yPlanet = self.EARTH_RADIUS*np.sin(theta)

		return {"planetX": xPlanet, "planetY": yPlanet}  
	

startTime = [ 0.0, 250.0 , 1100.0]
EndTime = [ 150.0, 350.0 , 1515.0]
SeperationTime = [ 5.0, 5.0, 5.0]
StageMass = [ 2.0, 1.0, 0.5]
StageThrust = [ 300.0, 300.0, 10.0]
ThrustAngle = [ 1.0, 75.0 , 150.0]
ISP = [ 1500.0, 2000.0, 3000.0]
rocketMass = 556000
text = OrbitOptimizer()

text.DefineRocketStages(
 3, startTime, EndTime, SeperationTime, StageMass, StageThrust, ThrustAngle, ISP, rocketMass
)

text.TrajectoryCalculation(
 50000, 0, 0, 0, 0
)
'''
OrbitOptimizer.ReadTxtFile(
	OrbitOptimizer(), "StandardAtmosphereModel.txt"
)
'''