import { useState } from "react";
import { ScrollView, StyleSheet, View } from "react-native";
import {
  ActivityIndicator,
  Appbar,
  Button,
  Card,
  Checkbox,
  Chip,
  Dialog,
  Divider,
  FAB,
  IconButton,
  List,
  Menu,
  Portal,
  ProgressBar,
  SegmentedButtons,
  Snackbar,
  Switch,
  Text,
  TextInput,
  useTheme,
} from "react-native-paper";
import { useSafeAreaInsets } from "react-native-safe-area-context";

// Тестовый экран для проверки совместимости Paper с Expo SDK 58 / RN 0.88.
export default function PaperDemoScreen() {
  const theme = useTheme();
  const insets = useSafeAreaInsets();

  const [name, setName] = useState("");
  const [weight, setWeight] = useState("");
  const [menuVisible, setMenuVisible] = useState(false);
  const [exercise, setExercise] = useState("Приседания");
  const [segment, setSegment] = useState("week");
  const [checked, setChecked] = useState(true);
  const [notify, setNotify] = useState(false);
  const [dialogVisible, setDialogVisible] = useState(false);
  const [snackVisible, setSnackVisible] = useState(false);

  return (
    <View style={[styles.root, { backgroundColor: theme.colors.background }]}>
      <Appbar.Header>
        <Appbar.Content title="Paper demo" subtitle="SDK 58 / RN 0.88" />
        <Appbar.Action icon="magnify" onPress={() => {}} />
        <Appbar.Action icon="dots-vertical" onPress={() => {}} />
      </Appbar.Header>

      <ScrollView
        contentContainerStyle={[
          styles.content,
          { paddingBottom: insets.bottom + 96 },
        ]}
      >
        <Text variant="titleLarge">Кнопки</Text>
        <View style={styles.row}>
          <Button mode="contained" onPress={() => {}}>
            Contained
          </Button>
          <Button mode="outlined" icon="plus" onPress={() => {}}>
            Outlined
          </Button>
          <Button mode="text" onPress={() => {}}>
            Text
          </Button>
          <IconButton icon="heart" onPress={() => {}} />
        </View>

        <Divider />

        <Text variant="titleLarge">Поля ввода</Text>
        <TextInput
          label="Название тренировки"
          value={name}
          onChangeText={setName}
          mode="outlined"
        />
        <TextInput
          label="Вес"
          value={weight}
          onChangeText={setWeight}
          keyboardType="numeric"
          right={<TextInput.Affix text="кг" />}
        />

        <Menu
          visible={menuVisible}
          onDismiss={() => setMenuVisible(false)}
          anchor={
            <Button
              mode="outlined"
              icon="chevron-down"
              onPress={() => setMenuVisible(true)}
            >
              {exercise}
            </Button>
          }
        >
          {["Приседания", "Жим лёжа", "Становая тяга"].map((item) => (
            <Menu.Item
              key={item}
              title={item}
              onPress={() => {
                setExercise(item);
                setMenuVisible(false);
              }}
            />
          ))}
        </Menu>

        <Divider />

        <Text variant="titleLarge">Выбор</Text>
        <SegmentedButtons
          value={segment}
          onValueChange={setSegment}
          buttons={[
            { value: "week", label: "Неделя" },
            { value: "month", label: "Месяц" },
            { value: "year", label: "Год" },
          ]}
        />
        <View style={styles.row}>
          <Chip icon="dumbbell">Силовая</Chip>
          <Chip icon="run" selected>
            Кардио
          </Chip>
        </View>
        <List.Item
          title="Напоминания"
          left={(p) => <List.Icon {...p} icon="bell" />}
          right={() => <Switch value={notify} onValueChange={setNotify} />}
        />
        <List.Item
          title="Разминка выполнена"
          left={(p) => <List.Icon {...p} icon="fire" />}
          right={() => (
            <Checkbox
              status={checked ? "checked" : "unchecked"}
              onPress={() => setChecked(!checked)}
            />
          )}
        />

        <Divider />

        <Text variant="titleLarge">Карточка и прогресс</Text>
        <Card>
          <Card.Title
            title="Жим лёжа"
            subtitle="3 подхода × 10 повторений"
            left={(p) => <List.Icon {...p} icon="weight-lifter" />}
          />
          <Card.Content>
            <ProgressBar progress={0.66} style={styles.progress} />
            <ActivityIndicator animating />
          </Card.Content>
          <Card.Actions>
            <Button onPress={() => setDialogVisible(true)}>Диалог</Button>
            <Button onPress={() => setSnackVisible(true)}>Снекбар</Button>
          </Card.Actions>
        </Card>
      </ScrollView>

      <FAB
        icon="plus"
        style={[styles.fab, { bottom: insets.bottom + 16 }]}
        onPress={() => setSnackVisible(true)}
      />

      <Portal>
        <Dialog visible={dialogVisible} onDismiss={() => setDialogVisible(false)}>
          <Dialog.Title>Завершить тренировку?</Dialog.Title>
          <Dialog.Content>
            <Text>Результаты будут сохранены в дневник.</Text>
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={() => setDialogVisible(false)}>Отмена</Button>
            <Button onPress={() => setDialogVisible(false)}>Да</Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>

      <Snackbar
        visible={snackVisible}
        onDismiss={() => setSnackVisible(false)}
        action={{ label: "OK", onPress: () => setSnackVisible(false) }}
      >
        Тренировка добавлена
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1 },
  content: { padding: 16, gap: 16 },
  row: { flexDirection: "row", flexWrap: "wrap", alignItems: "center", gap: 8 },
  progress: { marginBottom: 12 },
  fab: { position: "absolute", right: 16 },
});
