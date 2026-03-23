import { createNativeStackNavigator } from '@react-navigation/native-stack';
import LoginPage from '../screens/loginScreen';

const Stack = createNativeStackNavigator();

export default function TabNavigator() { // ← T maiúsculo
  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      <Stack.Screen name="Login" component={LoginPage} />
    </Stack.Navigator>
  );
}