import { useEffect } from 'react';
import * as SplashScreen from 'expo-splash-screen';
import { NavigationContainer } from '@react-navigation/native';
import TabNavigator from './src/nav/tabNavigator';

SplashScreen.preventAutoHideAsync();

export default function App() {
  useEffect(() => {
    // Esconde a splash nativa rapidinho
    SplashScreen.hideAsync();
  }, []);

  return (
    <NavigationContainer>
      <TabNavigator />
    </NavigationContainer>
  );
}