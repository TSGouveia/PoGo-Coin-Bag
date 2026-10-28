package io.github.naji.pokemongo.coin.bag

import android.Manifest
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import io.github.naji.pokemongo.coin.bag.ui.WearStatusScreen
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch

class MainActivity : ComponentActivity() {

    private val bleClient by lazy { BleClient(applicationContext) }
    private var scanJob: Job? = null
    private var permissionDenied by mutableStateOf(false)
    private var isManualScanning by mutableStateOf(false)

    private val requiredPermissions: Array<String>
        get() = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            arrayOf(Manifest.permission.BLUETOOTH_SCAN, Manifest.permission.BLUETOOTH_CONNECT)
        } else {
            arrayOf(Manifest.permission.ACCESS_FINE_LOCATION)
        }

    private val permissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestMultiplePermissions(),
    ) { grants ->
        permissionDenied = !grants.values.all { it }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            val state by bleClient.state.collectAsState()
            WearStatusScreen(
                state = state,
                permissionDenied = permissionDenied,
                isScanning = isManualScanning,
                onToggleScan = {
                    if (isManualScanning) {
                        stopScan()
                    } else {
                        requestAndStartScan()
                    }
                }
            )
        }
    }

    override fun onResume() {
        super.onResume()
        if (hasAllPermissions()) {
            permissionDenied = false
        }
    }

    override fun onPause() {
        super.onPause()
    }

    private fun hasAllPermissions(): Boolean =
        requiredPermissions.all {
            ContextCompat.checkSelfPermission(this, it) == PackageManager.PERMISSION_GRANTED
        }

    private fun requestAndStartScan() {
        if (hasAllPermissions()) {
            startScan()
        } else {
            permissionLauncher.launch(requiredPermissions)
        }
    }

    private fun startScan() {
        scanJob?.cancel()
        isManualScanning = true
        scanJob = lifecycleScope.launch {
            try {
                bleClient.run()
            } finally {
                isManualScanning = false
            }
        }
    }

    private fun stopScan() {
        scanJob?.cancel()
        bleClient.cancel()
        scanJob = null
        isManualScanning = false
    }
}
