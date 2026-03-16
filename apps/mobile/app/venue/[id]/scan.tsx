import { useState, useEffect } from 'react';
import { View, Text, Pressable, Alert } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { useCheckIn, CheckInError } from '../../../features/venue/hooks/use-checkin';
import { useLocation } from '../../../shared/hooks/use-location';

export default function QRScanScreen() {
  const { id: venueId } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const [permission, requestPermission] = useCameraPermissions();
  const [scanned, setScanned] = useState(false);
  const { latitude, longitude } = useLocation();
  const checkInMutation = useCheckIn();

  const handleBarCodeScanned = async (data: string) => {
    if (scanned || checkInMutation.isPending) return;
    setScanned(true);

    try {
      await checkInMutation.mutateAsync({
        qrPayload: data,
        lat: latitude,
        lng: longitude,
        deviceFingerprint: 'mobile-device',
        isAnonymous: false,
      });

      router.replace('/(tabs)/hub');
    } catch (err) {
      let title = 'Xəta';
      let message = 'Xəta baş verdi. Yenidən cəhd edin.';

      if (err instanceof CheckInError) {
        switch (err.code) {
          case 'INVALID_QR':
            title = 'QR etibarsızdır';
            message = 'QR kod etibarsızdır və ya vaxtı keçib.';
            break;
          case 'OUTSIDE_GEOFENCE':
            title = 'Məkandan kənardasınız';
            message = 'Məkana yaxınlaşın və yenidən cəhd edin.';
            break;
          case 'RATE_LIMITED':
            title = 'Gözləyin';
            message = '5 dəqiqə gözlədikdən sonra yenidən cəhd edin.';
            break;
          default:
            message = err.message;
        }
      }

      Alert.alert(title, message, [
        {
          text: 'Yenidən cəhd et',
          onPress: () => setScanned(false),
        },
        {
          text: 'Geri',
          onPress: () => router.back(),
          style: 'cancel',
        },
      ]);
    }
  };

  // Permission not yet determined
  if (!permission) {
    return (
      <View className="flex-1 bg-black items-center justify-center">
        <Text className="text-white text-base">Yüklənir...</Text>
      </View>
    );
  }

  // Permission denied
  if (!permission.granted) {
    return (
      <View className="flex-1 bg-background-light dark:bg-background-dark items-center justify-center px-8">
        <Text className="text-5xl mb-4">📷</Text>
        <Text className="text-xl font-semibold text-primary dark:text-white text-center mb-2">
          Kamera icazəsi lazımdır
        </Text>
        <Text className="text-sm text-gray-500 text-center mb-6">
          QR kodu scan etmək üçün kameranıza giriş icazəsi verin
        </Text>
        <Pressable
          onPress={requestPermission}
          className="bg-accent rounded-xl py-3 px-8"
          accessibilityRole="button"
          accessibilityLabel="Kamera icazesi ver"
        >
          <Text className="text-white font-semibold text-base">
            İcazə ver
          </Text>
        </Pressable>
        <Pressable
          onPress={() => router.back()}
          className="mt-4 py-3 px-8"
          accessibilityRole="button"
          accessibilityLabel="Geri"
        >
          <Text className="text-gray-500 text-base">Geri</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-black">
      <CameraView
        className="flex-1"
        barcodeScannerSettings={{ barcodeTypes: ['qr'] }}
        onBarcodeScanned={
          scanned
            ? undefined
            : (result) => handleBarCodeScanned(result.data)
        }
      >
        {/* Overlay */}
        <View className="flex-1 items-center justify-center">
          {/* Top area */}
          <View className="flex-1 w-full bg-black/50" />

          {/* Scanner frame row */}
          <View className="flex-row">
            <View className="flex-1 bg-black/50" />
            <View className="w-64 h-64 border-2 border-white rounded-2xl" />
            <View className="flex-1 bg-black/50" />
          </View>

          {/* Bottom area */}
          <View className="flex-1 w-full bg-black/50 items-center pt-8">
            <Text className="text-white text-base font-medium mb-2">
              {scanned ? 'Yoxlanılır...' : 'QR kodu çərçivəyə yönəldin'}
            </Text>
            {checkInMutation.isPending && (
              <Text className="text-white/70 text-sm">
                Check-in aparılır...
              </Text>
            )}
          </View>
        </View>
      </CameraView>

      {/* Close button */}
      <Pressable
        onPress={() => router.back()}
        className="absolute top-12 left-4 w-10 h-10 rounded-full bg-black/40 items-center justify-center"
        accessibilityRole="button"
        accessibilityLabel="Bağla"
      >
        <Text className="text-white text-lg font-bold">X</Text>
      </Pressable>
    </View>
  );
}
