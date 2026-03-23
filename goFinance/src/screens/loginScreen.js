import React from "react";
import { View, Text, Image, Button, TouchableOpacity } from "react-native";

export default function LoginPage() {
  return (
    <View>
      <Image source={require("../../assets/logo_light.png")} />
      <Text>goFinance</Text>
      <Text>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</Text>

      <Button title="Login" onPress={() => {}} />
      <Button title="Cadastrar-se" onPress={() => {}} />

      <Text>Esqueceu sua senha?</Text>
    </View>
  );
}