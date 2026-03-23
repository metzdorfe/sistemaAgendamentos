import React, { useEffect, useRef } from 'react';
import { View, Animated } from 'react-native';

export default function SplashScreen({ navigation }) {
  const opacity = useRef(new Animated.Value(1)).current; // já começa visível
  const scale = useRef(new Animated.Value(1)).current;   // já no tamanho final

  useEffect(() => {
    setTimeout(() => {
      // Animação de saída — logo some e encolhe
      Animated.parallel([
        Animated.timing(opacity, {
          toValue: 0,
          duration: 600,
          useNativeDriver: true,
        }),
        Animated.timing(scale, {
          toValue: 1.2, // cresce um pouco antes de sumir
          duration: 600,
          useNativeDriver: true,
        }),
      ]).start(() => {
        navigation.replace('Login');
      });
    }, 2000); // fica 2s parado antes de animar
  }, []);

  return (
    <View style={{
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      backgroundColor: '#0094D0', // mesma cor do app.json
    }}>
      <Animated.Image
        source={require('../../assets/logo_dark.png')}
        style={{
          width: 180,
          height: 180,
          opacity: opacity,
          transform: [{ scale: scale }],
        }}
        resizeMode="contain"
      />
    </View>
  );
}